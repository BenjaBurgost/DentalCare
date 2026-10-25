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
public class PacientesController : ControllerBase
{
    private readonly IPacienteService _pacienteService;

    public PacientesController(IPacienteService pacienteService)
    {
        _pacienteService = pacienteService;
    }

    /// <summary>Lista todos los pacientes (solo odontólogos).</summary>
    [HttpGet]
    [Authorize(Roles = Roles.Odontologo)]
    public async Task<ActionResult<IReadOnlyList<PacienteDto>>> GetAll(CancellationToken ct)
        => Ok(await _pacienteService.GetAllAsync(ct));

    /// <summary>Obtiene un paciente. Un paciente solo puede verse a sí mismo.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PacienteDto>> GetById(int id, CancellationToken ct)
        => Ok(await _pacienteService.GetByIdAsync(id, User.ToUsuarioActual(), ct));

    /// <summary>Da de alta un paciente (solo odontólogos; los pacientes usan /api/auth/register).</summary>
    [HttpPost]
    [Authorize(Roles = Roles.Odontologo)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PacienteDto>> Create(PacienteCreateDto dto, CancellationToken ct)
    {
        var paciente = await _pacienteService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = paciente.Id }, paciente);
    }

    /// <summary>Modifica los datos de un paciente. Un paciente solo puede modificarse a sí mismo.</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<PacienteDto>> Update(int id, PacienteUpdateDto dto, CancellationToken ct)
        => Ok(await _pacienteService.UpdateAsync(id, dto, User.ToUsuarioActual(), ct));

    /// <summary>Elimina un paciente sin turnos (solo odontólogos).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Odontologo)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _pacienteService.DeleteAsync(id, ct);
        return NoContent();
    }
}
