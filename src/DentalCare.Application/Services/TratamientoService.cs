using DentalCare.Application.Interfaces;
using DentalCare.Domain.Interfaces;

namespace DentalCare.Application.Services;

public class TratamientoService : ITratamientoService
{
    private readonly ITratamientoRepository _tratamientoRepository;

    public TratamientoService(ITratamientoRepository tratamientoRepository)
    {
        _tratamientoRepository = tratamientoRepository;
    }
}
