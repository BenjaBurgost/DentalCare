using DentalCare.Domain.Entities;
using DentalCare.Domain.Interfaces;
using DentalCare.Infrastructure.Data;

namespace DentalCare.Infrastructure.Repositories;

public class TratamientoRepository : Repository<Tratamiento>, ITratamientoRepository
{
    public TratamientoRepository(DentalCareDbContext context) : base(context)
    {
    }
}
