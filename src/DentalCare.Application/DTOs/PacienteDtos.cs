using System.ComponentModel.DataAnnotations;

namespace DentalCare.Application.DTOs;

public record PacienteDto(
    int Id,
    string Nombre,
    string Apellido,
    string Dni,
    DateOnly FechaNacimiento,
    string? Telefono,
    string Email,
    string? Direccion,
    string? ObraSocial,
    string? NumeroAfiliado);

public class PacienteUpdateDto
{
    [Required, StringLength(100)] public string Nombre { get; set; } = string.Empty;
    [Required, StringLength(100)] public string Apellido { get; set; } = string.Empty;
    [Required, StringLength(20, MinimumLength = 7)] public string Dni { get; set; } = string.Empty;
    [Required] public DateOnly FechaNacimiento { get; set; }
    [StringLength(30)] public string? Telefono { get; set; }
    [Required, EmailAddress, StringLength(150)] public string Email { get; set; } = string.Empty;
    [StringLength(200)] public string? Direccion { get; set; }
    [StringLength(100)] public string? ObraSocial { get; set; }
    [StringLength(50)] public string? NumeroAfiliado { get; set; }
}

public class PacienteCreateDto : PacienteUpdateDto
{
    [Required, StringLength(100, MinimumLength = 8)] public string Password { get; set; } = string.Empty;
}
