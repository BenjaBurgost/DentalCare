using DentalCare.Application.DTOs;
using DentalCare.Application.Interfaces;
using DentalCare.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DentalCare.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class OdontologosController : ControllerBase
{
    private readonly IOdontologoService _odontologoService;

    public OdontologosController(IOdontologoService odontologoService)
    {
        _odontologoService = odontologoService;
    }

    /// <summary>Lista los odontólogos.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OdontologoDto>>> GetAll(CancellationToken ct)
        => Ok(await _odontologoService.GetAllAsync(ct));

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OdontologoDto>> GetById(int id, CancellationToken ct)
        => Ok(await _odontologoService.GetByIdAsync(id, ct));

    /// <summary>Alta (solo odontólogos).</summary>
    [HttpPost]
    [Authorize(Roles = Roles.Odontologo)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OdontologoDto>> Create(OdontologoCreateDto dto, CancellationToken ct)
    {
        var creado = await _odontologoService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
    }

    /// <summary>Modificación (solo odontólogos).</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Odontologo)]
    public async Task<ActionResult<OdontologoDto>> Update(int id, OdontologoUpdateDto dto, CancellationToken ct)
        => Ok(await _odontologoService.UpdateAsync(id, dto, ct));

    /// <summary>Baja (solo odontólogos; no se permite si tiene turnos asociados).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Odontologo)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _odontologoService.DeleteAsync(id, ct);
        return NoContent();
    }
}
