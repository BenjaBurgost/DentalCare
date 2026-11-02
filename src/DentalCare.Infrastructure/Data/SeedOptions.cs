namespace DentalCare.Infrastructure.Data;

/// <summary>
/// Configuración del seed (sección "Seed" de appsettings). El odontólogo admin se crea siempre que tenga
/// contraseña configurada; los datos de prueba (segundo odontólogo, pacientes, consultorios y tratamientos)
/// solo se cargan si <see cref="DatosDePrueba"/> está en true, lo que únicamente ocurre en Development.
/// Las contraseñas salen de acá (configuración de Development), nunca del código ni de appsettings.json.
/// </summary>
public class SeedOptions
{
    public const string Section = "Seed";

    /// <summary>Odontólogo administrador inicial (en producción llega por variables de ambiente).</summary>
    public SeedOdontologo Odontologo { get; set; } = new();

    /// <summary>Interruptor del set de datos de prueba. Solo se activa en appsettings.Development.json.</summary>
    public bool DatosDePrueba { get; set; }

    public List<SeedOdontologo> Odontologos { get; set; } = new();
    public List<SeedPaciente> Pacientes { get; set; } = new();
    public List<SeedConsultorio> Consultorios { get; set; } = new();
    public List<SeedTratamiento> Tratamientos { get; set; } = new();
}

public class SeedOdontologo
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Dni { get; set; } = string.Empty;
    public string Matricula { get; set; } = string.Empty;
    public string Especialidad { get; set; } = string.Empty;
    public string? Telefono { get; set; }
}

public class SeedPaciente
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Dni { get; set; } = string.Empty;
    public DateOnly FechaNacimiento { get; set; }
    public string? Telefono { get; set; }
    public string? ObraSocial { get; set; }
    public string? NumeroAfiliado { get; set; }
}

public class SeedConsultorio
{
    public int Numero { get; set; }
    public int Piso { get; set; }
    public string? Nombre { get; set; }
    public string? Equipamiento { get; set; }
}

public class SeedTratamiento
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Costo { get; set; }
    public int DuracionEstimada { get; set; }
}
