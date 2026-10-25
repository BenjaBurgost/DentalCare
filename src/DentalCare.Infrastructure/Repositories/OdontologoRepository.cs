using DentalCare.Domain.Entities;
using DentalCare.Domain.Interfaces;
using DentalCare.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DentalCare.Infrastructure.Repositories;

public class OdontologoRepository : Repository<Odontologo>, IOdontologoRepository
{
    public OdontologoRepository(DentalCareDbContext context) : base(context)
    {
    }

    public Task<Odontologo?> GetByEmailAsync(string email, CancellationToken ct = default)
        => _dbSet.FirstOrDefaultAsync(o => o.Email == email, ct);
}
