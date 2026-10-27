using DentalCare.Application.Common;
using DentalCare.Application.DTOs;
using DentalCare.Application.Services;
using DentalCare.Domain.Entities;
using DentalCare.Domain.Enums;
using DentalCare.Domain.Exceptions;
using DentalCare.Infrastructure.Data;
using DentalCare.Infrastructure.Repositories;
using Microsoft.Extensions.Time.Testing;

namespace DentalCare.Tests;

public class TurnoServiceTests : IDisposable
{
    // "Hoy" es lunes 2 de noviembre de 2026 a las 9:00.
    private static readonly DateOnly Martes = new(2026, 11, 3);

    private readonly SqliteDb _db = new();
    private readonly DentalCareDbContext _ctx;
    private readonly FakeFeriadoService _feriados = new();
    private readonly TurnoService _service;
    private readonly UsuarioActual _odontologo = new(1, Roles.Odontologo);

    private int _pacienteId, _otroPacienteId, _odontologoId, _otroOdontologoId, _consultorioId, _otroConsultorioId, _limpiezaId, _extraccionId;

    public TurnoServiceTests()
    {
        _ctx = _db.CreateContext();
        Sembrar();

        var reloj = new FakeTimeProvider(new DateTimeOffset(2026, 11, 2, 9, 0, 0, TimeSpan.Zero));
        reloj.SetLocalTimeZone(TimeZoneInfo.Utc);

        _service = new TurnoService(
            new TurnoRepository(_ctx), new PacienteRepository(_ctx), new OdontologoRepository(_ctx),
            new ConsultorioRepository(_ctx), new TratamientoRepository(_ctx), _feriados, reloj);
    }

    private void Sembrar()
    {
        Paciente P(string dni) => new() { Nombre = "P", Apellido = dni, Dni = dni, Email = $"{dni}@p.com", PasswordHash = "x" };
        Odontologo O(string m) => new() { Nombre = "O", Apellido = m, Dni = m, Matricula = m, Especialidad = "General", Email = $"{m}@o.com", PasswordHash = "x" };

        var p1 = P("1111111"); var p2 = P("2222222");
        var o1 = O("MP1"); var o2 = O("MP2");
        var c1 = new Consultorio { Numero = 1, Piso = 1 }; var c2 = new Consultorio { Numero = 2, Piso = 1 };
        var limpieza = new Tratamiento { Codigo = "LIM", Nombre = "Limpieza", Costo = 100, DuracionEstimada = 30 };
        var extraccion = new Tratamiento { Codigo = "EXT", Nombre = "Extracción", Costo = 250, DuracionEstimada = 45 };
        _ctx.AddRange(p1, p2, o1, o2, c1, c2, limpieza, extraccion);
        _ctx.SaveChanges();

        (_pacienteId, _otroPacienteId) = (p1.Id, p2.Id);
        (_odontologoId, _otroOdontologoId) = (o1.Id, o2.Id);
        (_consultorioId, _otroConsultorioId) = (c1.Id, c2.Id);
        (_limpiezaId, _extraccionId) = (limpieza.Id, extraccion.Id);
    }

    private TurnoCreateDto Nuevo(string hora, int? odontologo = null, int? consultorio = null, DateOnly? fecha = null, params int[] tratamientos) => new()
    {
        Fecha = fecha ?? Martes,
        Hora = TimeOnly.Parse(hora),
        PacienteId = _pacienteId,
        OdontologoId = odontologo ?? _odontologoId,
        ConsultorioId = consultorio ?? _consultorioId,
        TratamientoIds = tratamientos.Length > 0 ? tratamientos.ToList() : new List<int> { _limpiezaId }
    };

    [Fact]
    public async Task Crear_CalculaDuracionYCosto_DesdeLosTratamientos()
    {
        var turno = await _service.CreateAsync(Nuevo("10:00", tratamientos: new[] { _limpiezaId, _extraccionId }), _odontologo);

        Assert.Equal(75, turno.Duracion);
        Assert.Equal(350, turno.CostoTotal);
        Assert.Equal(EstadoTurno.Pendiente, turno.Estado);
        Assert.Equal(2, turno.Tratamientos.Count);
    }

    [Fact]
    public async Task Crear_ComoPaciente_UsaSiempreElPacienteAutenticado()
    {
        var dto = Nuevo("10:00");
        dto.PacienteId = _pacienteId;

        var turno = await _service.CreateAsync(dto, new UsuarioActual(_otroPacienteId, Roles.Paciente));

        Assert.Equal(_otroPacienteId, turno.PacienteId);
    }

    [Theory]
    [InlineData("10:15", true, false)]  // mismo odontólogo, otro consultorio
    [InlineData("10:15", false, true)]  // otro odontólogo, mismo consultorio
    public async Task Crear_Superpuesto_Falla(string hora, bool mismoOdontologo, bool mismoConsultorio)
    {
        await _service.CreateAsync(Nuevo("10:00"), _odontologo);

        var dto = Nuevo(hora,
            mismoOdontologo ? _odontologoId : _otroOdontologoId,
            mismoConsultorio ? _consultorioId : _otroConsultorioId);

        await Assert.ThrowsAsync<DomainException>(() => _service.CreateAsync(dto, _odontologo));
    }

    [Fact]
    public async Task Crear_JustoDespuesDeOtroTurno_EsValido()
    {
        await _service.CreateAsync(Nuevo("10:00"), _odontologo);
        var segundo = await _service.CreateAsync(Nuevo("10:30"), _odontologo);
        Assert.Equal(new TimeOnly(10, 30), segundo.Hora);
    }

    [Fact]
    public async Task Crear_EnHorarioDeUnTurnoCancelado_EsValido()
    {
        var primero = await _service.CreateAsync(Nuevo("10:00"), _odontologo);
        await _service.CancelarAsync(primero.Id, _odontologo);

        var nuevo = await _service.CreateAsync(Nuevo("10:00"), _odontologo);
        Assert.NotEqual(primero.Id, nuevo.Id);
    }

    [Fact]
    public async Task Crear_EnFeriado_Falla()
    {
        _feriados.Feriados.Add(new FeriadoDto(Martes, "Holiday", "Feriado de prueba"));
        var ex = await Assert.ThrowsAsync<DomainException>(() => _service.CreateAsync(Nuevo("10:00"), _odontologo));
        Assert.Contains("feriado", ex.Message);
    }

    [Theory]
    [InlineData("07:30")]
    [InlineData("19:45")] // termina 20:15
    public async Task Crear_FueraDeHorario_Falla(string hora)
        => await Assert.ThrowsAsync<DomainException>(() => _service.CreateAsync(Nuevo(hora), _odontologo));

    [Fact]
    public async Task Crear_EnElPasadoOEnDomingo_Falla()
    {
        await Assert.ThrowsAsync<DomainException>(() => _service.CreateAsync(Nuevo("08:30", fecha: new DateOnly(2026, 11, 2)), _odontologo));
        await Assert.ThrowsAsync<DomainException>(() => _service.CreateAsync(Nuevo("10:00", fecha: new DateOnly(2026, 11, 8)), _odontologo));
    }

    [Fact]
    public async Task Crear_ConTratamientoInexistente_Falla()
        => await Assert.ThrowsAsync<DomainException>(() => _service.CreateAsync(Nuevo("10:00", tratamientos: new[] { _limpiezaId, 999 }), _odontologo));

    [Fact]
    public async Task Crear_ConOdontologoInexistente_Falla()
        => await Assert.ThrowsAsync<NotFoundException>(() => _service.CreateAsync(Nuevo("10:00", odontologo: 999), _odontologo));

    [Fact]
    public async Task Paciente_NoPuedeVerTurnosAjenos()
    {
        var turno = await _service.CreateAsync(Nuevo("10:00"), _odontologo);
        var otro = new UsuarioActual(_otroPacienteId, Roles.Paciente);

        await Assert.ThrowsAsync<ForbiddenException>(() => _service.GetByIdAsync(turno.Id, otro));
        Assert.Empty(await _service.SearchAsync(new TurnoFiltroDto { PacienteId = _pacienteId }, otro));
    }

    [Fact]
    public async Task Crear_SinTratamientos_Falla()
    {
        var dto = Nuevo("10:00");
        dto.TratamientoIds = new List<int>();
        await Assert.ThrowsAsync<DomainException>(() => _service.CreateAsync(dto, _odontologo));
    }

    [Fact]
    public async Task Crear_ConDuracionExplicita_IgnoraLaSumaDeTratamientos()
    {
        var dto = Nuevo("10:00", tratamientos: new[] { _limpiezaId, _extraccionId }); // suma estimada: 75
        dto.Duracion = 20;

        var turno = await _service.CreateAsync(dto, _odontologo);

        Assert.Equal(20, turno.Duracion);
    }

    [Theory]
    [InlineData(EstadoTurno.Finalizado)]
    [InlineData(EstadoTurno.Ausente)]
    public async Task CambiarEstado_NoPermiteFinalizarNiMarcarAusenteUnTurnoFuturo(EstadoTurno estado)
    {
        var turno = await _service.CreateAsync(Nuevo("10:00"), _odontologo);
        await _service.CambiarEstadoAsync(turno.Id, new TurnoCambioEstadoDto { Estado = EstadoTurno.Confirmado }, _odontologo);

        await Assert.ThrowsAsync<DomainException>(() =>
            _service.CambiarEstadoAsync(turno.Id, new TurnoCambioEstadoDto { Estado = estado }, _odontologo));
    }

    [Fact]
    public async Task CambiarEstado_FinalizaUnTurnoYaOcurrido()
    {
        // Viernes 30/10/2026: ya pasó respecto del "hoy" (lunes 02/11/2026 09:00).
        var turno = SembrarTurno(new DateOnly(2026, 10, 30), new TimeOnly(10, 0), EstadoTurno.Confirmado);

        var finalizado = await _service.CambiarEstadoAsync(
            turno.Id, new TurnoCambioEstadoDto { Estado = EstadoTurno.Finalizado }, _odontologo);

        Assert.Equal(EstadoTurno.Finalizado, finalizado.Estado);
    }

    /// <summary>Inserta un turno directamente en la BD (el servicio no permite crearlos en el pasado).</summary>
    private Turno SembrarTurno(DateOnly fecha, TimeOnly hora, EstadoTurno estado)
    {
        var turno = new Turno
        {
            Fecha = fecha,
            Hora = hora,
            Duracion = 30,
            Estado = estado,
            PacienteId = _pacienteId,
            OdontologoId = _odontologoId,
            ConsultorioId = _consultorioId
        };
        _ctx.Turnos.Add(turno);
        _ctx.SaveChanges();
        return turno;
    }

    [Fact]
    public async Task CambiarEstado_RespetaElCicloDeVida()
    {
        var turno = await _service.CreateAsync(Nuevo("10:00"), _odontologo);

        // Pendiente -> Finalizado no es válido
        await Assert.ThrowsAsync<DomainException>(() =>
            _service.CambiarEstadoAsync(turno.Id, new TurnoCambioEstadoDto { Estado = EstadoTurno.Finalizado }, _odontologo));

        var confirmado = await _service.CambiarEstadoAsync(turno.Id, new TurnoCambioEstadoDto { Estado = EstadoTurno.Confirmado }, _odontologo);
        Assert.Equal(EstadoTurno.Confirmado, confirmado.Estado);

        // Un paciente no puede confirmar, solo cancelar
        var paciente = new UsuarioActual(_pacienteId, Roles.Paciente);
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _service.CambiarEstadoAsync(turno.Id, new TurnoCambioEstadoDto { Estado = EstadoTurno.Finalizado }, paciente));
        var cancelado = await _service.CancelarAsync(turno.Id, paciente);
        Assert.Equal(EstadoTurno.Cancelado, cancelado.Estado);
    }

    public void Dispose()
    {
        _ctx.Dispose();
        _db.Dispose();
    }
}
