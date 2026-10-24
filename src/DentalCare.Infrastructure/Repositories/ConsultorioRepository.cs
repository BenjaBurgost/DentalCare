using DentalCare.Domain.Entities;
using DentalCare.Domain.Interfaces;
using DentalCare.Infrastructure.Data;

namespace DentalCare.Infrastructure.Repositories;

public class ConsultorioRepository : Repository<Consultorio>, IConsultorioRepository
{
    public ConsultorioRepository(DentalCareDbContext context) : base(context)
    {
    }
}
