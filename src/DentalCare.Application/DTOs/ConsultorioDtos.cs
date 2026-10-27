using System.ComponentModel.DataAnnotations;

namespace DentalCare.Application.DTOs;

public record ConsultorioDto(int Id, int Numero, int Piso, string? Nombre, string? Equipamiento);

public class ConsultorioSaveDto
{
    [Range(1, 9999)] public int Numero { get; set; }
    [Range(0, 200)] public int Piso { get; set; }
    [StringLength(100)] public string? Nombre { get; set; }
    [StringLength(500)] public string? Equipamiento { get; set; }
}
