namespace DentalCare.Domain.Entities;

public class Odontologo
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Dni { get; set; } = string.Empty;
    public string Matricula { get; set; } = string.Empty;
    public string Especialidad { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string Email { get; set; } = string.Empty;

    /// <summary>Un odontólogo atiende 0..* turnos.</summary>
    public ICollection<Turno> Turnos { get; set; } = new List<Turno>();
}
