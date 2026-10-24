using DentalCare.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DentalCare.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OdontologosController : ControllerBase
{
    private readonly IOdontologoService _odontologoService;

    public OdontologosController(IOdontologoService odontologoService)
    {
        _odontologoService = odontologoService;
    }
}
