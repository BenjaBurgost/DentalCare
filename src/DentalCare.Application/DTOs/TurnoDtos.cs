using System.ComponentModel.DataAnnotations;
using DentalCare.Domain.Enums;

namespace DentalCare.Application.DTOs;

public record TurnoDto(
    int Id,
    DateOnly Fecha,
    TimeOnly Hora,
    int Duracion,
    EstadoTurno Estado,
    string? Motivo,
    string? Observaciones,
    int PacienteId,
    string Paciente,
    int OdontologoId,
    string Odontologo,
    int ConsultorioId,
    string Consultorio,
    IReadOnlyList<TratamientoDto> Tratamientos,
    decimal CostoTotal);

public class TurnoCreateDto
{
    [Required] public DateOnly Fecha { get; set; }
    [Required] public TimeOnly Hora { get; set; }

    /// <summary>Minutos. Si se omite, se usa la suma de la duración estimada de los tratamientos.</summary>
    [Range(5, 480)] public int? Duracion { get; set; }

    [StringLength(255)] public string? Motivo { get; set; }
    [StringLength(1000)] public string? Observaciones { get; set; }

    /// <summary>Si quien reserva es un paciente, se ignora y se usa el paciente autenticado.</summary>
    public int PacienteId { get; set; }

    [Range(1, int.MaxValue)] public int OdontologoId { get; set; }
    [Range(1, int.MaxValue)] public int ConsultorioId { get; set; }

    /// <summary>Un turno incluye 1..* tratamientos.</summary>
    [Required, MinLength(1)] public List<int> TratamientoIds { get; set; } = new();
}

public class TurnoCambioEstadoDto
{
    [Required] public EstadoTurno Estado { get; set; }
    [StringLength(1000)] public string? Observaciones { get; set; }
}

public class TurnoFiltroDto
{
    public int? PacienteId { get; set; }
    public int? OdontologoId { get; set; }
    public int? ConsultorioId { get; set; }
    public DateOnly? Desde { get; set; }
    public DateOnly? Hasta { get; set; }
    public EstadoTurno? Estado { get; set; }
}
