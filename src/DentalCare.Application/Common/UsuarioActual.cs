using DentalCare.Domain.Enums;

namespace DentalCare.Application.Common;

/// <summary>Datos del usuario autenticado (salen de los claims del JWT).</summary>
public record UsuarioActual(int Id, string Rol)
{
    public bool EsOdontologo => Rol == Roles.Odontologo;
    public bool EsPaciente => Rol == Roles.Paciente;
}
