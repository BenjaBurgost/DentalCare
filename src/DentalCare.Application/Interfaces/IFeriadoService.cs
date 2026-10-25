using DentalCare.Application.DTOs;

namespace DentalCare.Application.Interfaces;

/// <summary>Consulta de feriados nacionales (servicio de terceros: Nager.Date).</summary>
public interface IFeriadoService
{
    Task<IReadOnlyList<FeriadoDto>> GetFeriadosAsync(int anio, CancellationToken ct = default);

    /// <summary>Devuelve el feriado de esa fecha o null si es día hábil.</summary>
    Task<FeriadoDto?> GetFeriadoAsync(DateOnly fecha, CancellationToken ct = default);
}
