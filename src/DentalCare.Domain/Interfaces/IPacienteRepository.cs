using DentalCare.Domain.Entities;

namespace DentalCare.Domain.Interfaces;

public interface IPacienteRepository : IRepository<Paciente>
{
    Task<Paciente?> GetByEmailAsync(string email, CancellationToken ct = default);
}
