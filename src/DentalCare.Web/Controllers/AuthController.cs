using DentalCare.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DentalCare.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // CU01: Iniciar sesión (pendiente de implementación)
    [HttpPost("login")]
    public IActionResult Login() => StatusCode(StatusCodes.Status501NotImplemented);

    // CU02: Registrar paciente (pendiente de implementación)
    [HttpPost("register")]
    public IActionResult Register() => StatusCode(StatusCodes.Status501NotImplemented);

    // CU03: Restablecer contraseña (pendiente de implementación)
    [HttpPost("forgot-password")]
    public IActionResult ForgotPassword() => StatusCode(StatusCodes.Status501NotImplemented);
}
