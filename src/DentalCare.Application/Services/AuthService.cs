using DentalCare.Application.DTOs;
using DentalCare.Application.Interfaces;
using DentalCare.Domain.Enums;
using DentalCare.Domain.Exceptions;
using DentalCare.Domain.Interfaces;

namespace DentalCare.Application.Services;

/// <summary>
/// Autenticación de pacientes y odontólogos por email + contraseña.
/// El rol del token depende de en qué tabla se encuentra el email.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IPacienteRepository _pacienteRepository;
    private readonly IOdontologoRepository _odontologoRepository;
    private readonly IPacienteService _pacienteService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public AuthService(
        IPacienteRepository pacienteRepository,
        IOdontologoRepository odontologoRepository,
        IPacienteService pacienteService,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator tokenGenerator)
    {
        _pacienteRepository = pacienteRepository;
        _odontologoRepository = odontologoRepository;
        _pacienteService = pacienteService;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
    }

    // CU01: Iniciar sesión
    public async Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        var odontologo = await _odontologoRepository.GetByEmailAsync(email, ct);
        if (odontologo is not null && _passwordHasher.Verify(odontologo.PasswordHash, dto.Password))
            return CrearRespuesta(odontologo.Id, $"{odontologo.Nombre} {odontologo.Apellido}", odontologo.Email, Roles.Odontologo);

        var paciente = await _pacienteRepository.GetByEmailAsync(email, ct);
        if (paciente is not null && _passwordHasher.Verify(paciente.PasswordHash, dto.Password))
            return CrearRespuesta(paciente.Id, $"{paciente.Nombre} {paciente.Apellido}", paciente.Email, Roles.Paciente);

        throw new UnauthorizedException("Email o contraseña incorrectos.");
    }

    // CU02: Registrar paciente
    public async Task<AuthResponseDto> RegisterAsync(PacienteCreateDto dto, CancellationToken ct = default)
    {
        var paciente = await _pacienteService.CreateAsync(dto, ct);
        return CrearRespuesta(paciente.Id, $"{paciente.Nombre} {paciente.Apellido}", paciente.Email, Roles.Paciente);
    }

    // CU03: Restablecer contraseña (paso 1: pedir el token)
    public async Task<string?> ForgotPasswordAsync(ForgotPasswordDto dto, CancellationToken ct = default)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        var odontologo = await _odontologoRepository.GetByEmailAsync(email, ct);
        if (odontologo is not null)
            return _tokenGenerator.GenerarResetToken(odontologo.Id, Roles.Odontologo, odontologo.PasswordHash);

        var paciente = await _pacienteRepository.GetByEmailAsync(email, ct);
        if (paciente is not null)
            return _tokenGenerator.GenerarResetToken(paciente.Id, Roles.Paciente, paciente.PasswordHash);

        // No se revela si el email existe o no.
        return null;
    }

    // CU03: Restablecer contraseña (paso 2: usar el token)
    public async Task ResetPasswordAsync(ResetPasswordDto dto, CancellationToken ct = default)
    {
        var info = _tokenGenerator.ValidarResetToken(dto.Token)
                   ?? throw new DomainException("El token de restablecimiento es inválido o expiró.");

        var nuevoHash = _passwordHasher.Hash(dto.NuevaPassword);

        if (info.Rol == Roles.Odontologo)
        {
            var odontologo = await _odontologoRepository.GetByIdAsync(info.UsuarioId, ct);
            if (odontologo is null || _tokenGenerator.CalcularSello(odontologo.PasswordHash) != info.Sello)
                throw new DomainException("El token de restablecimiento es inválido o ya fue utilizado.");
            odontologo.PasswordHash = nuevoHash;
            await _odontologoRepository.SaveChangesAsync(ct);
        }
        else
        {
            var paciente = await _pacienteRepository.GetByIdAsync(info.UsuarioId, ct);
            if (paciente is null || _tokenGenerator.CalcularSello(paciente.PasswordHash) != info.Sello)
                throw new DomainException("El token de restablecimiento es inválido o ya fue utilizado.");
            paciente.PasswordHash = nuevoHash;
            await _pacienteRepository.SaveChangesAsync(ct);
        }
    }

    private AuthResponseDto CrearRespuesta(int id, string nombre, string email, string rol)
    {
        var (token, expira) = _tokenGenerator.GenerarToken(id, nombre, email, rol);
        return new AuthResponseDto(token, expira, id, nombre, email, rol);
    }
}
