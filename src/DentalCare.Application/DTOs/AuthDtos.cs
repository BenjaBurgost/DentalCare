using System.ComponentModel.DataAnnotations;

namespace DentalCare.Application.DTOs;

public class LoginDto
{
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
}

public record AuthResponseDto(string Token, DateTime Expira, int UsuarioId, string Nombre, string Email, string Rol);

public class ForgotPasswordDto
{
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Respuesta de "olvidé mi contraseña". El token de restablecimiento solo se devuelve
/// en Development (en producción se enviaría por email).
/// </summary>
public record ForgotPasswordResponseDto(string Mensaje, string? ResetToken);

public class ResetPasswordDto
{
    [Required] public string Token { get; set; } = string.Empty;
    [Required, StringLength(100, MinimumLength = 8)] public string NuevaPassword { get; set; } = string.Empty;
}
