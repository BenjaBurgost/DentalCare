namespace DentalCare.Domain.Entities;

public class Turno
{
    public int Id { get; set; }
    public DateOnly Fecha { get; set; }
    public TimeOnly Hora { get; set; }

    /// <summary>Duración en minutos.</summary>
    public int Duracion { get; set; }

    /// <summary>Pendiente, Confirmado, Ausente, Finalizado o Cancelado.</summary>
    public string Estado { get; set; } = "Pendiente";
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
}
