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
public class TratamientosController : ControllerBase
{
    private readonly ITratamientoService _tratamientoService;

    public TratamientosController(ITratamientoService tratamientoService)
    {
        _tratamientoService = tratamientoService;
    }

    /// <summary>Lista los tratamientos.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TratamientoDto>>> GetAll(CancellationToken ct)
        => Ok(await _tratamientoService.GetAllAsync(ct));

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TratamientoDto>> GetById(int id, CancellationToken ct)
        => Ok(await _tratamientoService.GetByIdAsync(id, ct));

    /// <summary>Alta (solo odontólogos).</summary>
    [HttpPost]
    [Authorize(Roles = Roles.Odontologo)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TratamientoDto>> Create(TratamientoSaveDto dto, CancellationToken ct)
    {
        var creado = await _tratamientoService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
    }

    /// <summary>Modificación (solo odontólogos).</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Odontologo)]
    public async Task<ActionResult<TratamientoDto>> Update(int id, TratamientoSaveDto dto, CancellationToken ct)
        => Ok(await _tratamientoService.UpdateAsync(id, dto, ct));

    /// <summary>Baja (solo odontólogos; no se permite si tiene turnos asociados).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Odontologo)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _tratamientoService.DeleteAsync(id, ct);
        return NoContent();
    }
}
