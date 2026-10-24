using DentalCare.Application.Interfaces;
using DentalCare.Domain.Interfaces;

namespace DentalCare.Application.Services;

public class AuthService : IAuthService
{
    private readonly IPacienteRepository _pacienteRepository;
    private readonly IOdontologoRepository _odontologoRepository;

    public AuthService(IPacienteRepository pacienteRepository, IOdontologoRepository odontologoRepository)
    {
        _pacienteRepository = pacienteRepository;
        _odontologoRepository = odontologoRepository;
    }
}
