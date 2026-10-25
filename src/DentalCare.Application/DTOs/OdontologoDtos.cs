using System.ComponentModel.DataAnnotations;

namespace DentalCare.Application.DTOs;

public record OdontologoDto(
    int Id,
    string Nombre,
    string Apellido,
    string Dni,
    string Matricula,
    string Especialidad,
    string? Telefono,
    string Email);

public class OdontologoUpdateDto
{
    [Required, StringLength(100)] public string Nombre { get; set; } = string.Empty;
    [Required, StringLength(100)] public string Apellido { get; set; } = string.Empty;
    [Required, StringLength(20, MinimumLength = 7)] public string Dni { get; set; } = string.Empty;
    [Required, StringLength(50)] public string Matricula { get; set; } = string.Empty;
    [Required, StringLength(50)] public string Especialidad { get; set; } = string.Empty;
    [StringLength(30)] public string? Telefono { get; set; }
    [Required, EmailAddress, StringLength(150)] public string Email { get; set; } = string.Empty;
}

public class OdontologoCreateDto : OdontologoUpdateDto
{
    [Required, StringLength(100, MinimumLength = 8)] public string Password { get; set; } = string.Empty;
}
