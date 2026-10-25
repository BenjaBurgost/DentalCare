using System.ComponentModel.DataAnnotations;

namespace DentalCare.Application.DTOs;

public record TratamientoDto(int Id, string Codigo, string Nombre, string? Descripcion, decimal Costo, int DuracionEstimada);

public class TratamientoSaveDto
{
    [Required, StringLength(50)] public string Codigo { get; set; } = string.Empty;
    [Required, StringLength(100)] public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    [Range(0, 99999999)] public decimal Costo { get; set; }

    /// <summary>Duración estimada en minutos.</summary>
    [Range(5, 480)] public int DuracionEstimada { get; set; }
}
