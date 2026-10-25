using DentalCare.Application.Common;
using DentalCare.Application.DTOs;

namespace DentalCare.Application.Interfaces;

public interface IPacienteService
{
    Task<IReadOnlyList<PacienteDto>> GetAllAsync(CancellationToken ct = default);
    Task<PacienteDto> GetByIdAsync(int id, UsuarioActual usuario, CancellationToken ct = default);
    Task<PacienteDto> CreateAsync(PacienteCreateDto dto, CancellationToken ct = default);
    Task<PacienteDto> UpdateAsync(int id, PacienteUpdateDto dto, UsuarioActual usuario, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
