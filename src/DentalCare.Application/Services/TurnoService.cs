using DentalCare.Application.Common;
using DentalCare.Application.DTOs;
using DentalCare.Application.Interfaces;
using DentalCare.Domain.Entities;
using DentalCare.Domain.Enums;
using DentalCare.Domain.Exceptions;
using DentalCare.Domain.Interfaces;

namespace DentalCare.Application.Services;

public class TurnoService : ITurnoService
{
    /// <summary>Horario de atención del consultorio.</summary>
    public static readonly TimeOnly HoraApertura = new(8, 0);
    public static readonly TimeOnly HoraCierre = new(20, 0);

    private readonly ITurnoRepository _turnoRepository;
    private readonly IPacienteRepository _pacienteRepository;
    private readonly IOdontologoRepository _odontologoRepository;
    private readonly IConsultorioRepository _consultorioRepository;
    private readonly ITratamientoRepository _tratamientoRepository;
    private readonly IFeriadoService _feriadoService;
    private readonly TimeProvider _timeProvider;

    public TurnoService(
        ITurnoRepository turnoRepository,
        IPacienteRepository pacienteRepository,
        IOdontologoRepository odontologoRepository,
        IConsultorioRepository consultorioRepository,
        ITratamientoRepository tratamientoRepository,
        IFeriadoService feriadoService,
        TimeProvider timeProvider)
    {
        _turnoRepository = turnoRepository;
        _pacienteRepository = pacienteRepository;
        _odontologoRepository = odontologoRepository;
        _consultorioRepository = consultorioRepository;
        _tratamientoRepository = tratamientoRepository;
        _feriadoService = feriadoService;
        _timeProvider = timeProvider;
    }

    public async Task<IReadOnlyList<TurnoDto>> SearchAsync(TurnoFiltroDto filtro, UsuarioActual usuario, CancellationToken ct = default)
    {
        // Un paciente solo puede ver sus propios turnos, sin importar el filtro que envíe.
        var pacienteId = usuario.EsPaciente ? usuario.Id : filtro.PacienteId;

        var turnos = await _turnoRepository.SearchAsync(
            pacienteId, filtro.OdontologoId, filtro.ConsultorioId, filtro.Desde, filtro.Hasta, filtro.Estado, ct);

        return turnos.Select(t => t.ToDto()).ToList();
    }

    public async Task<TurnoDto> GetByIdAsync(int id, UsuarioActual usuario, CancellationToken ct = default)
    {
        var turno = await ObtenerAsync(id, usuario, ct);
        return turno.ToDto();
    }

    public async Task<TurnoDto> CreateAsync(TurnoCreateDto dto, UsuarioActual usuario, CancellationToken ct = default)
    {
        var pacienteId = usuario.EsPaciente ? usuario.Id : dto.PacienteId;

        if (!await _pacienteRepository.ExistsAsync(p => p.Id == pacienteId, ct))
            throw new NotFoundException("Paciente", pacienteId);
        if (!await _odontologoRepository.ExistsAsync(o => o.Id == dto.OdontologoId, ct))
            throw new NotFoundException("Odontólogo", dto.OdontologoId);
        if (!await _consultorioRepository.ExistsAsync(c => c.Id == dto.ConsultorioId, ct))
            throw new NotFoundException("Consultorio", dto.ConsultorioId);

        var tratamientos = await _tratamientoRepository.GetByIdsAsync(dto.TratamientoIds, ct);
        var faltantes = dto.TratamientoIds.Distinct().Except(tratamientos.Select(t => t.Id)).ToList();
        if (tratamientos.Count == 0 || faltantes.Count > 0)
            throw new DomainException($"Tratamientos inexistentes: {string.Join(", ", faltantes)}.");

        var turno = new Turno
        {
            Fecha = dto.Fecha,
            Hora = dto.Hora,
            Duracion = dto.Duracion ?? tratamientos.Sum(t => t.DuracionEstimada),
            Estado = EstadoTurno.Pendiente,
            Motivo = dto.Motivo,
            Observaciones = dto.Observaciones,
            PacienteId = pacienteId,
            OdontologoId = dto.OdontologoId,
            ConsultorioId = dto.ConsultorioId,
            Tratamientos = tratamientos.ToList()
        };

        await ValidarHorarioAsync(turno, ct);

        await _turnoRepository.AddAsync(turno, ct);
        await _turnoRepository.SaveChangesAsync(ct);

        var creado = await _turnoRepository.GetByIdWithDetailsAsync(turno.Id, ct);
        return creado!.ToDto();
    }

    public async Task<TurnoDto> CambiarEstadoAsync(int id, TurnoCambioEstadoDto dto, UsuarioActual usuario, CancellationToken ct = default)
    {
        var turno = await ObtenerAsync(id, usuario, ct);

        if (usuario.EsPaciente && dto.Estado != EstadoTurno.Cancelado)
            throw new ForbiddenException("Un paciente solo puede cancelar sus turnos.");

        if (!turno.PuedeCambiarA(dto.Estado))
            throw new DomainException($"No se puede pasar un turno de {turno.Estado} a {dto.Estado}.");

        if (dto.Estado is EstadoTurno.Finalizado or EstadoTurno.Ausente
            && turno.FechaHoraInicio > Ahora())
            throw new DomainException($"No se puede marcar como {dto.Estado} un turno que todavía no ocurrió.");

        turno.Estado = dto.Estado;
        if (dto.Observaciones is not null)
            turno.Observaciones = dto.Observaciones;

        await _turnoRepository.SaveChangesAsync(ct);
        return turno.ToDto();
    }

    public Task<TurnoDto> CancelarAsync(int id, UsuarioActual usuario, CancellationToken ct = default)
        => CambiarEstadoAsync(id, new TurnoCambioEstadoDto { Estado = EstadoTurno.Cancelado }, usuario, ct);

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var turno = await _turnoRepository.GetByIdWithDetailsAsync(id, ct) ?? throw new NotFoundException("Turno", id);
        _turnoRepository.Remove(turno);
        await _turnoRepository.SaveChangesAsync(ct);
    }

    private async Task<Turno> ObtenerAsync(int id, UsuarioActual usuario, CancellationToken ct)
    {
        var turno = await _turnoRepository.GetByIdWithDetailsAsync(id, ct) ?? throw new NotFoundException("Turno", id);

        if (usuario.EsPaciente && turno.PacienteId != usuario.Id)
            throw new ForbiddenException("Un paciente solo puede acceder a sus propios turnos.");

        return turno;
    }

    private DateTime Ahora() => _timeProvider.GetLocalNow().DateTime;

    private async Task ValidarHorarioAsync(Turno turno, CancellationToken ct)
    {
        if (turno.FechaHoraInicio <= Ahora())
            throw new DomainException("No se pueden reservar turnos en el pasado.");

        if (turno.Fecha.DayOfWeek == DayOfWeek.Sunday)
            throw new DomainException("El consultorio no atiende los domingos.");

        var fin = TimeOnly.FromDateTime(turno.FechaHoraFin);
        if (turno.Hora < HoraApertura || turno.FechaHoraFin.Date != turno.FechaHoraInicio.Date || fin > HoraCierre)
            throw new DomainException($"El turno debe estar dentro del horario de atención ({HoraApertura:HH\\:mm} a {HoraCierre:HH\\:mm}).");

        var feriado = await _feriadoService.GetFeriadoAsync(turno.Fecha, ct);
        if (feriado is not null)
            throw new DomainException($"El {turno.Fecha:dd/MM/yyyy} es feriado ({feriado.NombreLocal}).");

        var delDia = await _turnoRepository.GetActivosDelDiaAsync(turno.Fecha, turno.OdontologoId, turno.ConsultorioId, ct);
        var superpuestos = delDia.Where(t => t.SeSuperponeCon(turno.FechaHoraInicio, turno.FechaHoraFin)).ToList();

        if (superpuestos.Any(t => t.OdontologoId == turno.OdontologoId))
            throw new DomainException("El odontólogo ya tiene un turno en ese horario.");
        if (superpuestos.Any(t => t.ConsultorioId == turno.ConsultorioId))
            throw new DomainException("El consultorio ya está ocupado en ese horario.");
    }
}
