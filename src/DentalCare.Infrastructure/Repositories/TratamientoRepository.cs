using DentalCare.Domain.Entities;
using DentalCare.Domain.Interfaces;
using DentalCare.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DentalCare.Infrastructure.Repositories;

public class TratamientoRepository : Repository<Tratamiento>, ITratamientoRepository
{
    public TratamientoRepository(DentalCareDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Tratamiento>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
    {
        var lista = ids.Distinct().ToList();
        return await _dbSet.Where(t => lista.Contains(t.Id)).ToListAsync(ct);
    }
}
