namespace DentalCare.Domain.Entities;

public class Paciente
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Dni { get; set; } = string.Empty;
    public DateOnly FechaNacimiento { get; set; }
    public string? Telefono { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? ObraSocial { get; set; }
    public string? NumeroAfiliado { get; set; }

    /// <summary>Un paciente solicita 0..* turnos.</summary>
    public ICollection<Turno> Turnos { get; set; } = new List<Turno>();
}
