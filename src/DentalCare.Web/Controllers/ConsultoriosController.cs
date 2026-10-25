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
public class ConsultoriosController : ControllerBase
{
    private readonly IConsultorioService _consultorioService;

    public ConsultoriosController(IConsultorioService consultorioService)
    {
        _consultorioService = consultorioService;
    }

    /// <summary>Lista los consultorios.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ConsultorioDto>>> GetAll(CancellationToken ct)
        => Ok(await _consultorioService.GetAllAsync(ct));

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConsultorioDto>> GetById(int id, CancellationToken ct)
        => Ok(await _consultorioService.GetByIdAsync(id, ct));

    /// <summary>Alta (solo odontólogos).</summary>
    [HttpPost]
    [Authorize(Roles = Roles.Odontologo)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ConsultorioDto>> Create(ConsultorioSaveDto dto, CancellationToken ct)
    {
        var creado = await _consultorioService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
    }

    /// <summary>Modificación (solo odontólogos).</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Odontologo)]
    public async Task<ActionResult<ConsultorioDto>> Update(int id, ConsultorioSaveDto dto, CancellationToken ct)
        => Ok(await _consultorioService.UpdateAsync(id, dto, ct));

    /// <summary>Baja (solo odontólogos; no se permite si tiene turnos asociados).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Odontologo)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _consultorioService.DeleteAsync(id, ct);
        return NoContent();
    }
}
