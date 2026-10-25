using DentalCare.Application.DTOs;

namespace DentalCare.Application.Interfaces;

public interface IOdontologoService
{
    Task<IReadOnlyList<OdontologoDto>> GetAllAsync(CancellationToken ct = default);
    Task<OdontologoDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<OdontologoDto> CreateAsync(OdontologoCreateDto dto, CancellationToken ct = default);
    Task<OdontologoDto> UpdateAsync(int id, OdontologoUpdateDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
