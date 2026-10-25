using DentalCare.Domain.Entities;

namespace DentalCare.Application.DTOs;

/// <summary>Conversión de entidades a DTOs (sin AutoMapper para mantenerlo simple).</summary>
public static class Mappings
{
    public static PacienteDto ToDto(this Paciente p) => new(
        p.Id, p.Nombre, p.Apellido, p.Dni, p.FechaNacimiento, p.Telefono, p.Email,
        p.Direccion, p.ObraSocial, p.NumeroAfiliado);

    public static OdontologoDto ToDto(this Odontologo o) => new(
        o.Id, o.Nombre, o.Apellido, o.Dni, o.Matricula, o.Especialidad, o.Telefono, o.Email);

    public static ConsultorioDto ToDto(this Consultorio c) => new(c.Id, c.Numero, c.Piso, c.Nombre, c.Equipamiento);

    public static TratamientoDto ToDto(this Tratamiento t) => new(
        t.Id, t.Codigo, t.Nombre, t.Descripcion, t.Costo, t.DuracionEstimada);

    public static TurnoDto ToDto(this Turno t) => new(
        t.Id,
        t.Fecha,
        t.Hora,
        t.Duracion,
        t.Estado,
        t.Motivo,
        t.Observaciones,
        t.PacienteId,
        $"{t.Paciente?.Apellido}, {t.Paciente?.Nombre}",
        t.OdontologoId,
        $"{t.Odontologo?.Apellido}, {t.Odontologo?.Nombre}",
        t.ConsultorioId,
        t.Consultorio is null ? string.Empty : t.Consultorio.Nombre ?? $"Consultorio {t.Consultorio.Numero} (piso {t.Consultorio.Piso})",
        t.Tratamientos.Select(x => x.ToDto()).ToList(),
        t.Tratamientos.Sum(x => x.Costo));
}
