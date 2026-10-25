using DentalCare.Application.DTOs;
using DentalCare.Application.Interfaces;
using DentalCare.Domain.Entities;
using DentalCare.Domain.Exceptions;
using DentalCare.Domain.Interfaces;

namespace DentalCare.Application.Services;

public class ConsultorioService : IConsultorioService
{
    private readonly IConsultorioRepository _consultorioRepository;
    private readonly ITurnoRepository _turnoRepository;

    public ConsultorioService(IConsultorioRepository consultorioRepository, ITurnoRepository turnoRepository)
    {
        _consultorioRepository = consultorioRepository;
        _turnoRepository = turnoRepository;
    }

    public async Task<IReadOnlyList<ConsultorioDto>> GetAllAsync(CancellationToken ct = default)
    {
        var consultorios = await _consultorioRepository.GetAllAsync(ct);
        return consultorios.OrderBy(c => c.Piso).ThenBy(c => c.Numero).Select(c => c.ToDto()).ToList();
    }

    public async Task<ConsultorioDto> GetByIdAsync(int id, CancellationToken ct = default)
        => (await ObtenerAsync(id, ct)).ToDto();

    public async Task<ConsultorioDto> CreateAsync(ConsultorioSaveDto dto, CancellationToken ct = default)
    {
        await ValidarUnicidadAsync(dto, null, ct);

        var consultorio = new Consultorio();
        Aplicar(consultorio, dto);

        await _consultorioRepository.AddAsync(consultorio, ct);
        await _consultorioRepository.SaveChangesAsync(ct);
        return consultorio.ToDto();
    }

    public async Task<ConsultorioDto> UpdateAsync(int id, ConsultorioSaveDto dto, CancellationToken ct = default)
    {
        var consultorio = await ObtenerAsync(id, ct);
        await ValidarUnicidadAsync(dto, id, ct);

        Aplicar(consultorio, dto);
        await _consultorioRepository.SaveChangesAsync(ct);
        return consultorio.ToDto();
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var consultorio = await ObtenerAsync(id, ct);

        if (await _turnoRepository.ExistsAsync(t => t.ConsultorioId == id, ct))
            throw new DomainException("No se puede eliminar un consultorio que tiene turnos registrados.");

        _consultorioRepository.Remove(consultorio);
        await _consultorioRepository.SaveChangesAsync(ct);
    }

    private async Task<Consultorio> ObtenerAsync(int id, CancellationToken ct)
        => await _consultorioRepository.GetByIdAsync(id, ct) ?? throw new NotFoundException("Consultorio", id);

    private async Task ValidarUnicidadAsync(ConsultorioSaveDto dto, int? idActual, CancellationToken ct)
    {
        if (await _consultorioRepository.ExistsAsync(c => c.Piso == dto.Piso && c.Numero == dto.Numero && c.Id != idActual, ct))
            throw new DomainException($"Ya existe el consultorio {dto.Numero} en el piso {dto.Piso}.");
    }

    private static void Aplicar(Consultorio c, ConsultorioSaveDto dto)
    {
        c.Numero = dto.Numero;
        c.Piso = dto.Piso;
        c.Nombre = dto.Nombre;
        c.Equipamiento = dto.Equipamiento;
    }
}
