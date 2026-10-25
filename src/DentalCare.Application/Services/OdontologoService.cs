using DentalCare.Application.DTOs;
using DentalCare.Application.Interfaces;
using DentalCare.Domain.Entities;
using DentalCare.Domain.Exceptions;
using DentalCare.Domain.Interfaces;

namespace DentalCare.Application.Services;

public class OdontologoService : IOdontologoService
{
    private readonly IOdontologoRepository _odontologoRepository;
    private readonly IPacienteRepository _pacienteRepository;
    private readonly ITurnoRepository _turnoRepository;
    private readonly IPasswordHasher _passwordHasher;

    public OdontologoService(
        IOdontologoRepository odontologoRepository,
        IPacienteRepository pacienteRepository,
        ITurnoRepository turnoRepository,
        IPasswordHasher passwordHasher)
    {
        _odontologoRepository = odontologoRepository;
        _pacienteRepository = pacienteRepository;
        _turnoRepository = turnoRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<IReadOnlyList<OdontologoDto>> GetAllAsync(CancellationToken ct = default)
    {
        var odontologos = await _odontologoRepository.GetAllAsync(ct);
        return odontologos.OrderBy(o => o.Apellido).ThenBy(o => o.Nombre).Select(o => o.ToDto()).ToList();
    }

    public async Task<OdontologoDto> GetByIdAsync(int id, CancellationToken ct = default)
        => (await ObtenerAsync(id, ct)).ToDto();

    public async Task<OdontologoDto> CreateAsync(OdontologoCreateDto dto, CancellationToken ct = default)
    {
        await ValidarUnicidadAsync(dto, null, ct);

        var odontologo = new Odontologo { PasswordHash = _passwordHasher.Hash(dto.Password) };
        Aplicar(odontologo, dto);

        await _odontologoRepository.AddAsync(odontologo, ct);
        await _odontologoRepository.SaveChangesAsync(ct);
        return odontologo.ToDto();
    }

    public async Task<OdontologoDto> UpdateAsync(int id, OdontologoUpdateDto dto, CancellationToken ct = default)
    {
        var odontologo = await ObtenerAsync(id, ct);
        await ValidarUnicidadAsync(dto, id, ct);

        Aplicar(odontologo, dto);
        await _odontologoRepository.SaveChangesAsync(ct);
        return odontologo.ToDto();
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var odontologo = await ObtenerAsync(id, ct);

        if (await _turnoRepository.ExistsAsync(t => t.OdontologoId == id, ct))
            throw new DomainException("No se puede eliminar un odontólogo que tiene turnos registrados.");

        _odontologoRepository.Remove(odontologo);
        await _odontologoRepository.SaveChangesAsync(ct);
    }

    private async Task<Odontologo> ObtenerAsync(int id, CancellationToken ct)
        => await _odontologoRepository.GetByIdAsync(id, ct) ?? throw new NotFoundException("Odontólogo", id);

    private async Task ValidarUnicidadAsync(OdontologoUpdateDto dto, int? idActual, CancellationToken ct)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        if (await _odontologoRepository.ExistsAsync(o => o.Dni == dto.Dni && o.Id != idActual, ct))
            throw new DomainException($"Ya existe un odontólogo con DNI {dto.Dni}.");

        if (await _odontologoRepository.ExistsAsync(o => o.Matricula == dto.Matricula && o.Id != idActual, ct))
            throw new DomainException($"Ya existe un odontólogo con matrícula {dto.Matricula}.");

        if (await _odontologoRepository.ExistsAsync(o => o.Email == email && o.Id != idActual, ct)
            || await _pacienteRepository.ExistsAsync(p => p.Email == email, ct))
            throw new DomainException($"El email {email} ya está registrado.");
    }

    private static void Aplicar(Odontologo o, OdontologoUpdateDto dto)
    {
        o.Nombre = dto.Nombre.Trim();
        o.Apellido = dto.Apellido.Trim();
        o.Dni = dto.Dni.Trim();
        o.Matricula = dto.Matricula.Trim();
        o.Especialidad = dto.Especialidad.Trim();
        o.Telefono = dto.Telefono;
        o.Email = dto.Email.Trim().ToLowerInvariant();
    }
}
