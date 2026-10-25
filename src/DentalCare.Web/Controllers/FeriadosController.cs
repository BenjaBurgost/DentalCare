using DentalCare.Application.DTOs;
using DentalCare.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DentalCare.Web.Controllers;

/// <summary>Feriados nacionales obtenidos del servicio de terceros Nager.Date (vía HttpClient Factory).</summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
[Produces("application/json")]
public class FeriadosController : ControllerBase
{
    private readonly IFeriadoService _feriadoService;

    public FeriadosController(IFeriadoService feriadoService)
    {
        _feriadoService = feriadoService;
    }

    /// <summary>Lista los feriados nacionales del año indicado.</summary>
    [HttpGet("{anio:int:range(2000,2100)}")]
    public async Task<ActionResult<IReadOnlyList<FeriadoDto>>> GetByAnio(int anio, CancellationToken ct)
        => Ok(await _feriadoService.GetFeriadosAsync(anio, ct));

    /// <summary>Indica si una fecha (yyyy-MM-dd) es feriado.</summary>
    [HttpGet("verificar")]
    public async Task<IActionResult> Verificar([FromQuery] DateOnly fecha, CancellationToken ct)
    {
        var feriado = await _feriadoService.GetFeriadoAsync(fecha, ct);
        return Ok(new { fecha, esFeriado = feriado is not null, feriado });
    }
}
