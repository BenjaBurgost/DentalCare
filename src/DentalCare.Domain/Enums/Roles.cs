namespace DentalCare.Domain.Enums;

/// <summary>Roles que se emiten como claim en el token JWT.</summary>
public static class Roles
{
    public const string Paciente = "Paciente";
    public const string Odontologo = "Odontologo";
}
