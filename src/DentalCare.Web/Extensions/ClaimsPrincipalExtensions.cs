using System.Security.Claims;
using DentalCare.Application.Common;

namespace DentalCare.Web.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>Arma el usuario actual a partir de los claims del token JWT.</summary>
    public static UsuarioActual ToUsuarioActual(this ClaimsPrincipal user)
    {
        var id = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub")
                 ?? throw new InvalidOperationException("El token no contiene el id del usuario.");
        var rol = user.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        return new UsuarioActual(int.Parse(id), rol);
    }
}
