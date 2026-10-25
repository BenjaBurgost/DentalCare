using DentalCare.Domain.Entities;

namespace DentalCare.Domain.Interfaces;

public interface ITratamientoRepository : IRepository<Tratamiento>
{
    /// <summary>Devuelve los tratamientos (con tracking) cuyos ids están en la lista.</summary>
    Task<IReadOnlyList<Tratamiento>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
}
