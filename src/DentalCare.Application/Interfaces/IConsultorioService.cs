using DentalCare.Application.DTOs;

namespace DentalCare.Application.Interfaces;

public interface IConsultorioService
{
    Task<IReadOnlyList<ConsultorioDto>> GetAllAsync(CancellationToken ct = default);
    Task<ConsultorioDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ConsultorioDto> CreateAsync(ConsultorioSaveDto dto, CancellationToken ct = default);
    Task<ConsultorioDto> UpdateAsync(int id, ConsultorioSaveDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
