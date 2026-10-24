using DentalCare.Application.Interfaces;
using DentalCare.Domain.Interfaces;

namespace DentalCare.Application.Services;

public class TurnoService : ITurnoService
{
    private readonly ITurnoRepository _turnoRepository;
    private readonly IPacienteRepository _pacienteRepository;
    private readonly IOdontologoRepository _odontologoRepository;
    private readonly IConsultorioRepository _consultorioRepository;
    private readonly ITratamientoRepository _tratamientoRepository;

    public TurnoService(
        ITurnoRepository turnoRepository,
        IPacienteRepository pacienteRepository,
        IOdontologoRepository odontologoRepository,
        IConsultorioRepository consultorioRepository,
        ITratamientoRepository tratamientoRepository)
    {
        _turnoRepository = turnoRepository;
        _pacienteRepository = pacienteRepository;
        _odontologoRepository = odontologoRepository;
        _consultorioRepository = consultorioRepository;
        _tratamientoRepository = tratamientoRepository;
    }
}
