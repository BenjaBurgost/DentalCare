using DentalCare.Domain.Entities;
using DentalCare.Domain.Enums;

namespace DentalCare.Domain.Interfaces;

public interface ITurnoRepository : IRepository<Turno>
{
    /// <summary>Turno con paciente, odontólogo, consultorio y tratamientos (con tracking).</summary>
    Task<Turno?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);

    Task<IReadOnlyList<Turno>> SearchAsync(
        int? pacienteId = null,
        int? odontologoId = null,
        int? consultorioId = null,
        DateOnly? desde = null,
        DateOnly? hasta = null,
        EstadoTurno? estado = null,
        CancellationToken ct = default);

    /// <summary>Turnos no cancelados de una fecha para un odontólogo o un consultorio.</summary>
    Task<IReadOnlyList<Turno>> GetActivosDelDiaAsync(
        DateOnly fecha, int odontologoId, int consultorioId, CancellationToken ct = default);
}
