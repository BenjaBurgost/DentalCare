using DentalCare.Application.Interfaces;
using DentalCare.Domain.Interfaces;

namespace DentalCare.Application.Services;

public class PacienteService : IPacienteService
{
    private readonly IPacienteRepository _pacienteRepository;

    public PacienteService(IPacienteRepository pacienteRepository)
    {
        _pacienteRepository = pacienteRepository;
    }
}
