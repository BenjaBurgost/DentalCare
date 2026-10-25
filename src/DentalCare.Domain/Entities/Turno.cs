using DentalCare.Domain.Enums;

namespace DentalCare.Domain.Entities;

public class Turno
{
    public int Id { get; set; }
    public DateOnly Fecha { get; set; }
    public TimeOnly Hora { get; set; }

    /// <summary>Duración en minutos.</summary>
    public int Duracion { get; set; }

    public EstadoTurno Estado { get; set; } = EstadoTurno.Pendiente;
    public string? Motivo { get; set; }
    public string? Observaciones { get; set; }

    // Paciente (1) solicita Turno (0..*)
    public int PacienteId { get; set; }
    public Paciente Paciente { get; set; } = null!;

    // Odontologo (1) atiende Turno (0..*)
    public int OdontologoId { get; set; }
    public Odontologo Odontologo { get; set; } = null!;

    // Consultorio (1) aloja Turno (0..*)
    public int ConsultorioId { get; set; }
    public Consultorio Consultorio { get; set; } = null!;

    // Turno (0..*) incluye Tratamiento (1..*) : N:N sin entidad intermedia
    public ICollection<Tratamiento> Tratamientos { get; set; } = new List<Tratamiento>();

    public DateTime FechaHoraInicio => Fecha.ToDateTime(Hora);
    public DateTime FechaHoraFin => FechaHoraInicio.AddMinutes(Duracion);

    /// <summary>Un turno cancelado no ocupa agenda ni consultorio.</summary>
    public bool OcupaAgenda => Estado != EstadoTurno.Cancelado;

    public bool SeSuperponeCon(DateTime inicio, DateTime fin)
        => OcupaAgenda && FechaHoraInicio < fin && inicio < FechaHoraFin;

    /// <summary>Transiciones válidas del ciclo de vida del turno.</summary>
    public bool PuedeCambiarA(EstadoTurno nuevo) => (Estado, nuevo) switch
    {
        (EstadoTurno.Pendiente, EstadoTurno.Confirmado) => true,
        (EstadoTurno.Pendiente, EstadoTurno.Cancelado) => true,
        (EstadoTurno.Confirmado, EstadoTurno.Finalizado) => true,
        (EstadoTurno.Confirmado, EstadoTurno.Ausente) => true,
        (EstadoTurno.Confirmado, EstadoTurno.Cancelado) => true,
        _ => false
    };
}
