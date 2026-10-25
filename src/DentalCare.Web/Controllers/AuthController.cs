using System.Security.Claims;
using DentalCare.Application.DTOs;
using DentalCare.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DentalCare.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, IWebHostEnvironment environment, ILogger<AuthController> logger)
    {
        _authService = authService;
        _environment = environment;
        _logger = logger;
    }

    /// <summary>CU01: Iniciar sesión (paciente u odontólogo). Devuelve el token JWT.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto, CancellationToken ct)
        => Ok(await _authService.LoginAsync(dto, ct));

    /// <summary>CU02: Registrar paciente. Devuelve el token JWT del paciente creado.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponseDto>> Register(PacienteCreateDto dto, CancellationToken ct)
    {
        var respuesta = await _authService.RegisterAsync(dto, ct);
        return CreatedAtAction(nameof(PacientesController.GetById), "Pacientes", new { id = respuesta.UsuarioId }, respuesta);
    }

    /// <summary>
    /// CU03 (paso 1): Solicitar el restablecimiento de contraseña. El token se enviaría por email;
    /// en Development se devuelve en la respuesta para poder probar el flujo.
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ForgotPasswordResponseDto), StatusCodes.Status202Accepted)]
    public async Task<ActionResult<ForgotPasswordResponseDto>> ForgotPassword(ForgotPasswordDto dto, CancellationToken ct)
    {
        var token = await _authService.ForgotPasswordAsync(dto, ct);
        if (token is not null)
            _logger.LogInformation("Token de restablecimiento generado para {Email}.", dto.Email);

        const string mensaje = "Si el email está registrado, se enviaron las instrucciones para restablecer la contraseña.";
        return Accepted(new ForgotPasswordResponseDto(mensaje, _environment.IsDevelopment() ? token : null));
    }

    /// <summary>CU03 (paso 2): Restablecer la contraseña con el token recibido.</summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto dto, CancellationToken ct)
    {
        await _authService.ResetPasswordAsync(dto, ct);
        return NoContent();
    }

    /// <summary>Datos del usuario autenticado según el token.</summary>
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me() => Ok(new
    {
        Id = User.FindFirstValue(ClaimTypes.NameIdentifier),
        Nombre = User.FindFirstValue(ClaimTypes.Name),
        Email = User.FindFirstValue(ClaimTypes.Email),
        Rol = User.FindFirstValue(ClaimTypes.Role)
    });
}
