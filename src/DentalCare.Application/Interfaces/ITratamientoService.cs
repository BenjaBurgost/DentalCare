using DentalCare.Application.DTOs;

namespace DentalCare.Application.Interfaces;

public interface ITratamientoService
{
    Task<IReadOnlyList<TratamientoDto>> GetAllAsync(CancellationToken ct = default);
    Task<TratamientoDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<TratamientoDto> CreateAsync(TratamientoSaveDto dto, CancellationToken ct = default);
    Task<TratamientoDto> UpdateAsync(int id, TratamientoSaveDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
