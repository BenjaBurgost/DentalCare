using DentalCare.Domain.Entities;
using DentalCare.Domain.Interfaces;
using DentalCare.Infrastructure.Data;

namespace DentalCare.Infrastructure.Repositories;

public class TurnoRepository : Repository<Turno>, ITurnoRepository
{
    public TurnoRepository(DentalCareDbContext context) : base(context)
    {
    }
}
