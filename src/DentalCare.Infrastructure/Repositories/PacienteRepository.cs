using DentalCare.Domain.Entities;
using DentalCare.Domain.Interfaces;
using DentalCare.Infrastructure.Data;

namespace DentalCare.Infrastructure.Repositories;

public class PacienteRepository : Repository<Paciente>, IPacienteRepository
{
    public PacienteRepository(DentalCareDbContext context) : base(context)
    {
    }
}
