using DentalCare.Domain.Entities;
using DentalCare.Domain.Interfaces;
using DentalCare.Infrastructure.Data;

namespace DentalCare.Infrastructure.Repositories;

public class OdontologoRepository : Repository<Odontologo>, IOdontologoRepository
{
    public OdontologoRepository(DentalCareDbContext context) : base(context)
    {
    }
}
