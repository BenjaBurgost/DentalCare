using DentalCare.Application.Common;
using DentalCare.Application.DTOs;

namespace DentalCare.Application.Interfaces;

public interface ITurnoService
{
    /// <summary>Un paciente solo ve sus propios turnos; un odontólogo puede filtrar libremente.</summary>
    Task<IReadOnlyList<TurnoDto>> SearchAsync(TurnoFiltroDto filtro, UsuarioActual usuario, CancellationToken ct = default);
    Task<TurnoDto> GetByIdAsync(int id, UsuarioActual usuario, CancellationToken ct = default);
    Task<TurnoDto> CreateAsync(TurnoCreateDto dto, UsuarioActual usuario, CancellationToken ct = default);
    Task<TurnoDto> CambiarEstadoAsync(int id, TurnoCambioEstadoDto dto, UsuarioActual usuario, CancellationToken ct = default);
    Task<TurnoDto> CancelarAsync(int id, UsuarioActual usuario, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
