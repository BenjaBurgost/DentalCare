using DentalCare.Domain.Entities;
using DentalCare.Domain.Interfaces;
using DentalCare.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DentalCare.Infrastructure.Repositories;

public class PacienteRepository : Repository<Paciente>, IPacienteRepository
{
    public PacienteRepository(DentalCareDbContext context) : base(context)
    {
    }

    public Task<Paciente?> GetByEmailAsync(string email, CancellationToken ct = default)
        => _dbSet.FirstOrDefaultAsync(p => p.Email == email, ct);
}
