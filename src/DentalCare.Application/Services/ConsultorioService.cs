using DentalCare.Application.Interfaces;
using DentalCare.Domain.Interfaces;

namespace DentalCare.Application.Services;

public class ConsultorioService : IConsultorioService
{
    private readonly IConsultorioRepository _consultorioRepository;

    public ConsultorioService(IConsultorioRepository consultorioRepository)
    {
        _consultorioRepository = consultorioRepository;
    }
}
