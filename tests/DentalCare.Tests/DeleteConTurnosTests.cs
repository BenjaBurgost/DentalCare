using DentalCare.Application.Services;
using DentalCare.Domain.Entities;
using DentalCare.Domain.Enums;
using DentalCare.Domain.Exceptions;
using DentalCare.Infrastructure.Data;
using DentalCare.Infrastructure.Repositories;
using DentalCare.Infrastructure.Security;

namespace DentalCare.Tests;

/// <summary>
/// Regla de negocio: no se puede eliminar un paciente, odontólogo, consultorio ni tratamiento
/// que tenga turnos asociados. Se siembra un turno que los relaciona a los cuatro.
/// </summary>
public class DeleteConTurnosTests : IDisposable
{
    private readonly SqliteDb _db = new();
    private readonly DentalCareDbContext _ctx;
    private readonly PacienteService _pacientes;
    private readonly OdontologoService _odontologos;
    private readonly ConsultorioService _consultorios;
    private readonly TratamientoService _tratamientos;

    private readonly int _pacienteId, _odontologoId, _consultorioId, _tratamientoId;

    public DeleteConTurnosTests()
    {
        _ctx = _db.CreateContext();

        var pacienteRepo = new PacienteRepository(_ctx);
        var odontologoRepo = new OdontologoRepository(_ctx);
        var consultorioRepo = new ConsultorioRepository(_ctx);
        var tratamientoRepo = new TratamientoRepository(_ctx);
        var turnoRepo = new TurnoRepository(_ctx);
        var hasher = new PasswordHasher();

        _pacientes = new PacienteService(pacienteRepo, odontologoRepo, turnoRepo, hasher);
        _odontologos = new OdontologoService(odontologoRepo, pacienteRepo, turnoRepo, hasher);
        _consultorios = new ConsultorioService(consultorioRepo, turnoRepo);
        _tratamientos = new TratamientoService(tratamientoRepo, turnoRepo);

        var paciente = new Paciente { Nombre = "Ana", Apellido = "Pérez", Dni = "30111222", Email = "ana@mail.com", PasswordHash = "x" };
        var odontologo = new Odontologo { Nombre = "Juan", Apellido = "Díaz", Dni = "20333444", Matricula = "MP1", Especialidad = "General", Email = "juan@mail.com", PasswordHash = "x" };
        var consultorio = new Consultorio { Numero = 1, Piso = 1 };
        var tratamiento = new Tratamiento { Codigo = "LIM", Nombre = "Limpieza", Costo = 100, DuracionEstimada = 30 };
        _ctx.AddRange(paciente, odontologo, consultorio, tratamiento);
        _ctx.SaveChanges();

        _ctx.Turnos.Add(new Turno
        {
            Fecha = new DateOnly(2026, 11, 3),
            Hora = new TimeOnly(10, 0),
            Duracion = 30,
            Estado = EstadoTurno.Pendiente,
            PacienteId = paciente.Id,
            OdontologoId = odontologo.Id,
            ConsultorioId = consultorio.Id,
            Tratamientos = { tratamiento }
        });
        _ctx.SaveChanges();

        (_pacienteId, _odontologoId) = (paciente.Id, odontologo.Id);
        (_consultorioId, _tratamientoId) = (consultorio.Id, tratamiento.Id);
    }

    [Fact]
    public async Task NoSePuedeBorrarPacienteConTurnos()
        => await Assert.ThrowsAsync<DomainException>(() => _pacientes.DeleteAsync(_pacienteId));

    [Fact]
    public async Task NoSePuedeBorrarOdontologoConTurnos()
        => await Assert.ThrowsAsync<DomainException>(() => _odontologos.DeleteAsync(_odontologoId));

    [Fact]
    public async Task NoSePuedeBorrarConsultorioConTurnos()
        => await Assert.ThrowsAsync<DomainException>(() => _consultorios.DeleteAsync(_consultorioId));

    [Fact]
    public async Task NoSePuedeBorrarTratamientoIncluidoEnTurnos()
        => await Assert.ThrowsAsync<DomainException>(() => _tratamientos.DeleteAsync(_tratamientoId));

    public void Dispose()
    {
        _ctx.Dispose();
        _db.Dispose();
    }
}
