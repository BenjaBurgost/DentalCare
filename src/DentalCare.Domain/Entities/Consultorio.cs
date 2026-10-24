namespace DentalCare.Domain.Entities;

public class Consultorio
{
    public int Id { get; set; }
    public int Numero { get; set; }
    public int Piso { get; set; }
    public string? Nombre { get; set; }
    public string? Equipamiento { get; set; }

    public ICollection<Turno> Turnos { get; set; } = new List<Turno>();
}
