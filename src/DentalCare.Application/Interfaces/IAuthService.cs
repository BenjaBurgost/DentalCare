using DentalCare.Application.DTOs;

namespace DentalCare.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken ct = default);
    Task<AuthResponseDto> RegisterAsync(PacienteCreateDto dto, CancellationToken ct = default);
    Task<string?> ForgotPasswordAsync(ForgotPasswordDto dto, CancellationToken ct = default);
    Task ResetPasswordAsync(ResetPasswordDto dto, CancellationToken ct = default);
}
