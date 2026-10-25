namespace DentalCare.Application.Interfaces;

public record ResetTokenInfo(int UsuarioId, string Rol, string Sello);

public interface IJwtTokenGenerator
{
    (string Token, DateTime Expira) GenerarToken(int usuarioId, string nombre, string email, string rol);

    /// <summary>
    /// Token de un solo uso para restablecer la contraseña: incluye un "sello" derivado del hash
    /// actual, por lo que deja de ser válido apenas se cambia la contraseña.
    /// </summary>
    string GenerarResetToken(int usuarioId, string rol, string passwordHashActual);

    ResetTokenInfo? ValidarResetToken(string token);

    string CalcularSello(string passwordHash);
}
