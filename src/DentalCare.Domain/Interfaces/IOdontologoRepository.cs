using DentalCare.Domain.Entities;

namespace DentalCare.Domain.Interfaces;

public interface IOdontologoRepository : IRepository<Odontologo>
{
    Task<Odontologo?> GetByEmailAsync(string email, CancellationToken ct = default);
}
