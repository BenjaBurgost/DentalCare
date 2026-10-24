using DentalCare.Application.Interfaces;
using DentalCare.Domain.Interfaces;

namespace DentalCare.Application.Services;

public class OdontologoService : IOdontologoService
{
    private readonly IOdontologoRepository _odontologoRepository;

    public OdontologoService(IOdontologoRepository odontologoRepository)
    {
        _odontologoRepository = odontologoRepository;
    }
}
