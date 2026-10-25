using DentalCare.Application.DTOs;
using DentalCare.Application.Interfaces;
using DentalCare.Domain.Enums;
using DentalCare.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DentalCare.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class TurnosController : ControllerBase
{
    private readonly ITurnoService _turnoService;

    public TurnosController(ITurnoService turnoService)
    {
        _turnoService = turnoService;
    }

    /// <summary>
    /// Busca turnos por paciente, odontólogo, consultorio, rango de fechas y estado.
    /// Un paciente solo recibe sus propios turnos.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TurnoDto>>> Search([FromQuery] TurnoFiltroDto filtro, CancellationToken ct)
        => Ok(await _turnoService.SearchAsync(filtro, User.ToUsuarioActual(), ct));

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TurnoDto>> GetById(int id, CancellationToken ct)
        => Ok(await _turnoService.GetByIdAsync(id, User.ToUsuarioActual(), ct));

    /// <summary>
    /// Reserva un turno. Valida horario de atención, feriados (servicio externo),
    /// y superposición con otros turnos del odontólogo o del consultorio.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TurnoDto>> Create(TurnoCreateDto dto, CancellationToken ct)
    {
        var turno = await _turnoService.CreateAsync(dto, User.ToUsuarioActual(), ct);
        return CreatedAtAction(nameof(GetById), new { id = turno.Id }, turno);
    }

    /// <summary>Cambia el estado del turno (Confirmado, Finalizado, Ausente, Cancelado).</summary>
    [HttpPatch("{id:int}/estado")]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TurnoDto>> CambiarEstado(int id, TurnoCambioEstadoDto dto, CancellationToken ct)
        => Ok(await _turnoService.CambiarEstadoAsync(id, dto, User.ToUsuarioActual(), ct));

    /// <summary>Cancela un turno (el paciente puede cancelar los suyos).</summary>
    [HttpPost("{id:int}/cancelar")]
    public async Task<ActionResult<TurnoDto>> Cancelar(int id, CancellationToken ct)
        => Ok(await _turnoService.CancelarAsync(id, User.ToUsuarioActual(), ct));

    /// <summary>Elimina definitivamente un turno (solo odontólogos).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Odontologo)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _turnoService.DeleteAsync(id, ct);
        return NoContent();
    }
}
