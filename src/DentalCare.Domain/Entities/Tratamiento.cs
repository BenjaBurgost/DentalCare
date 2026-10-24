namespace DentalCare.Domain.Entities;

public class Tratamiento
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Costo { get; set; }

    /// <summary>Duración estimada en minutos.</summary>
    public int DuracionEstimada { get; set; }

    /// <summary>Un tratamiento puede incluirse en muchos turnos (N:N).</summary>
    public ICollection<Turno> Turnos { get; set; } = new List<Turno>();
}
