using DentalCare.Domain.Entities;
using DentalCare.Domain.Enums;
using DentalCare.Domain.Interfaces;
using DentalCare.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DentalCare.Infrastructure.Repositories;

public class TurnoRepository : Repository<Turno>, ITurnoRepository
{
    public TurnoRepository(DentalCareDbContext context) : base(context)
    {
    }

    private IQueryable<Turno> ConDetalles() => _dbSet
        .Include(t => t.Paciente)
        .Include(t => t.Odontologo)
        .Include(t => t.Consultorio)
        .Include(t => t.Tratamientos);

    public Task<Turno?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default)
        => ConDetalles().FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<IReadOnlyList<Turno>> SearchAsync(
        int? pacienteId = null,
        int? odontologoId = null,
        int? consultorioId = null,
        DateOnly? desde = null,
        DateOnly? hasta = null,
        EstadoTurno? estado = null,
        CancellationToken ct = default)
    {
        var query = ConDetalles().AsNoTracking();

        if (pacienteId.HasValue) query = query.Where(t => t.PacienteId == pacienteId);
        if (odontologoId.HasValue) query = query.Where(t => t.OdontologoId == odontologoId);
        if (consultorioId.HasValue) query = query.Where(t => t.ConsultorioId == consultorioId);
        if (desde.HasValue) query = query.Where(t => t.Fecha >= desde);
        if (hasta.HasValue) query = query.Where(t => t.Fecha <= hasta);
        if (estado.HasValue) query = query.Where(t => t.Estado == estado);

        return await query
            .OrderBy(t => t.Fecha)
            .ThenBy(t => t.Hora)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Turno>> GetActivosDelDiaAsync(
        DateOnly fecha, int odontologoId, int consultorioId, CancellationToken ct = default)
        => await _dbSet.AsNoTracking()
            .Where(t => t.Fecha == fecha
                        && t.Estado != EstadoTurno.Cancelado
                        && (t.OdontologoId == odontologoId || t.ConsultorioId == consultorioId))
            .ToListAsync(ct);
}
