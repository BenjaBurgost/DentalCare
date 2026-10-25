using DentalCare.Application.Common;
using DentalCare.Application.DTOs;
using DentalCare.Application.Interfaces;
using DentalCare.Domain.Entities;
using DentalCare.Domain.Exceptions;
using DentalCare.Domain.Interfaces;

namespace DentalCare.Application.Services;

public class PacienteService : IPacienteService
{
    private readonly IPacienteRepository _pacienteRepository;
    private readonly IOdontologoRepository _odontologoRepository;
    private readonly ITurnoRepository _turnoRepository;
    private readonly IPasswordHasher _passwordHasher;

    public PacienteService(
        IPacienteRepository pacienteRepository,
        IOdontologoRepository odontologoRepository,
        ITurnoRepository turnoRepository,
        IPasswordHasher passwordHasher)
    {
        _pacienteRepository = pacienteRepository;
        _odontologoRepository = odontologoRepository;
        _turnoRepository = turnoRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<IReadOnlyList<PacienteDto>> GetAllAsync(CancellationToken ct = default)
    {
        var pacientes = await _pacienteRepository.GetAllAsync(ct);
        return pacientes.OrderBy(p => p.Apellido).ThenBy(p => p.Nombre).Select(p => p.ToDto()).ToList();
    }

    public async Task<PacienteDto> GetByIdAsync(int id, UsuarioActual usuario, CancellationToken ct = default)
    {
        VerificarAcceso(id, usuario);
        return (await ObtenerAsync(id, ct)).ToDto();
    }

    public async Task<PacienteDto> CreateAsync(PacienteCreateDto dto, CancellationToken ct = default)
    {
        await ValidarUnicidadAsync(dto, null, ct);

        var paciente = new Paciente { PasswordHash = _passwordHasher.Hash(dto.Password) };
        Aplicar(paciente, dto);

        await _pacienteRepository.AddAsync(paciente, ct);
        await _pacienteRepository.SaveChangesAsync(ct);
        return paciente.ToDto();
    }

    public async Task<PacienteDto> UpdateAsync(int id, PacienteUpdateDto dto, UsuarioActual usuario, CancellationToken ct = default)
    {
        VerificarAcceso(id, usuario);
        var paciente = await ObtenerAsync(id, ct);
        await ValidarUnicidadAsync(dto, id, ct);

        Aplicar(paciente, dto);
        await _pacienteRepository.SaveChangesAsync(ct);
        return paciente.ToDto();
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var paciente = await ObtenerAsync(id, ct);

        if (await _turnoRepository.ExistsAsync(t => t.PacienteId == id, ct))
            throw new DomainException("No se puede eliminar un paciente que tiene turnos registrados.");

        _pacienteRepository.Remove(paciente);
        await _pacienteRepository.SaveChangesAsync(ct);
    }

    private async Task<Paciente> ObtenerAsync(int id, CancellationToken ct)
        => await _pacienteRepository.GetByIdAsync(id, ct) ?? throw new NotFoundException("Paciente", id);

    private static void VerificarAcceso(int id, UsuarioActual usuario)
    {
        if (usuario.EsPaciente && usuario.Id != id)
            throw new ForbiddenException("Un paciente solo puede acceder a sus propios datos.");
    }

    private async Task ValidarUnicidadAsync(PacienteUpdateDto dto, int? idActual, CancellationToken ct)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        if (await _pacienteRepository.ExistsAsync(p => p.Dni == dto.Dni && p.Id != idActual, ct))
            throw new DomainException($"Ya existe un paciente con DNI {dto.Dni}.");

        if (await _pacienteRepository.ExistsAsync(p => p.Email == email && p.Id != idActual, ct)
            || await _odontologoRepository.ExistsAsync(o => o.Email == email, ct))
            throw new DomainException($"El email {email} ya está registrado.");
    }

    private static void Aplicar(Paciente p, PacienteUpdateDto dto)
    {
        p.Nombre = dto.Nombre.Trim();
        p.Apellido = dto.Apellido.Trim();
        p.Dni = dto.Dni.Trim();
        p.FechaNacimiento = dto.FechaNacimiento;
        p.Telefono = dto.Telefono;
        p.Email = dto.Email.Trim().ToLowerInvariant();
        p.Direccion = dto.Direccion;
        p.ObraSocial = dto.ObraSocial;
        p.NumeroAfiliado = dto.NumeroAfiliado;
    }
}
