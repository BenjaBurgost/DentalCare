using DentalCare.Application.DTOs;
using DentalCare.Application.Interfaces;
using DentalCare.Domain.Entities;
using DentalCare.Domain.Exceptions;
using DentalCare.Domain.Interfaces;

namespace DentalCare.Application.Services;

public class TratamientoService : ITratamientoService
{
    private readonly ITratamientoRepository _tratamientoRepository;
    private readonly ITurnoRepository _turnoRepository;

    public TratamientoService(ITratamientoRepository tratamientoRepository, ITurnoRepository turnoRepository)
    {
        _tratamientoRepository = tratamientoRepository;
        _turnoRepository = turnoRepository;
    }

    public async Task<IReadOnlyList<TratamientoDto>> GetAllAsync(CancellationToken ct = default)
    {
        var tratamientos = await _tratamientoRepository.GetAllAsync(ct);
        return tratamientos.OrderBy(t => t.Nombre).Select(t => t.ToDto()).ToList();
    }

    public async Task<TratamientoDto> GetByIdAsync(int id, CancellationToken ct = default)
        => (await ObtenerAsync(id, ct)).ToDto();

    public async Task<TratamientoDto> CreateAsync(TratamientoSaveDto dto, CancellationToken ct = default)
    {
        await ValidarUnicidadAsync(dto, null, ct);

        var tratamiento = new Tratamiento();
        Aplicar(tratamiento, dto);

        await _tratamientoRepository.AddAsync(tratamiento, ct);
        await _tratamientoRepository.SaveChangesAsync(ct);
        return tratamiento.ToDto();
    }

    public async Task<TratamientoDto> UpdateAsync(int id, TratamientoSaveDto dto, CancellationToken ct = default)
    {
        var tratamiento = await ObtenerAsync(id, ct);
        await ValidarUnicidadAsync(dto, id, ct);

        Aplicar(tratamiento, dto);
        await _tratamientoRepository.SaveChangesAsync(ct);
        return tratamiento.ToDto();
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var tratamiento = await ObtenerAsync(id, ct);

        if (await _turnoRepository.ExistsAsync(t => t.Tratamientos.Any(x => x.Id == id), ct))
            throw new DomainException("No se puede eliminar un tratamiento incluido en turnos.");

        _tratamientoRepository.Remove(tratamiento);
        await _tratamientoRepository.SaveChangesAsync(ct);
    }

    private async Task<Tratamiento> ObtenerAsync(int id, CancellationToken ct)
        => await _tratamientoRepository.GetByIdAsync(id, ct) ?? throw new NotFoundException("Tratamiento", id);

    private async Task ValidarUnicidadAsync(TratamientoSaveDto dto, int? idActual, CancellationToken ct)
    {
        var codigo = dto.Codigo.Trim().ToUpperInvariant();
        if (await _tratamientoRepository.ExistsAsync(t => t.Codigo == codigo && t.Id != idActual, ct))
            throw new DomainException($"Ya existe un tratamiento con código {codigo}.");
    }

    private static void Aplicar(Tratamiento t, TratamientoSaveDto dto)
    {
        t.Codigo = dto.Codigo.Trim().ToUpperInvariant();
        t.Nombre = dto.Nombre.Trim();
        t.Descripcion = dto.Descripcion;
        t.Costo = dto.Costo;
        t.DuracionEstimada = dto.DuracionEstimada;
    }
}
