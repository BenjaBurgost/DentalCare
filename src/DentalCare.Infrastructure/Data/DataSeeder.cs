using DentalCare.Application.Interfaces;
using DentalCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DentalCare.Infrastructure.Data;

/// <summary>
/// Aplica las migrations pendientes (si Database:MigrateOnStartup = true) y siembra datos.
/// Siempre intenta crear el odontólogo admin (si tiene contraseña configurada). Si Seed:DatosDePrueba
/// está activado (solo en Development), además crea un set de datos de prueba. Todo es idempotente:
/// no se duplica nada que ya exista (se compara por email, por piso+número o por código).
/// Las contraseñas provienen de la configuración de Development, nunca del código ni de appsettings.json.
/// </summary>
public static class DataSeeder
{
    public static async Task InicializarAsync(IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;
        var context = sp.GetRequiredService<DentalCareDbContext>();
        var config = sp.GetRequiredService<IConfiguration>();
        var hasher = sp.GetRequiredService<IPasswordHasher>();
        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(DataSeeder));

        if (config.GetValue<bool>("Database:MigrateOnStartup") && context.Database.IsRelational())
        {
            logger.LogInformation("Aplicando migrations pendientes...");
            await context.Database.MigrateAsync(ct);
        }

        var opciones = config.GetSection(SeedOptions.Section).Get<SeedOptions>() ?? new SeedOptions();

        // Odontólogo admin: se crea en cualquier entorno si tiene contraseña configurada.
        await SembrarOdontologoAsync(context, hasher, opciones.Odontologo, logger, ct);

        // Datos de prueba: solo si están habilitados por configuración (activado en Development).
        if (opciones.DatosDePrueba)
        {
            foreach (var o in opciones.Odontologos)
                await SembrarOdontologoAsync(context, hasher, o, logger, ct);

            foreach (var p in opciones.Pacientes)
                await SembrarPacienteAsync(context, hasher, p, logger, ct);

            foreach (var c in opciones.Consultorios)
                await SembrarConsultorioAsync(context, c, logger, ct);

            foreach (var t in opciones.Tratamientos)
                await SembrarTratamientoAsync(context, t, logger, ct);
        }

        await context.SaveChangesAsync(ct);
    }

    private static async Task SembrarOdontologoAsync(
        DentalCareDbContext ctx, IPasswordHasher hasher, SeedOdontologo o, ILogger logger, CancellationToken ct)
    {
        // Sin email o sin contraseña no se crea nada (ej.: producción sin variable de ambiente).
        if (string.IsNullOrWhiteSpace(o.Email) || string.IsNullOrWhiteSpace(o.Password))
            return;

        var email = o.Email.Trim().ToLowerInvariant();
        if (await ctx.Odontologos.AnyAsync(x => x.Email == email, ct))
            return; // ya existe: no se duplica

        ctx.Odontologos.Add(new Odontologo
        {
            Nombre = string.IsNullOrWhiteSpace(o.Nombre) ? "Administrador" : o.Nombre,
            Apellido = string.IsNullOrWhiteSpace(o.Apellido) ? "DentalCare" : o.Apellido,
            Dni = string.IsNullOrWhiteSpace(o.Dni) ? "00000000" : o.Dni,
            Matricula = string.IsNullOrWhiteSpace(o.Matricula) ? "MP-0000" : o.Matricula,
            Especialidad = string.IsNullOrWhiteSpace(o.Especialidad) ? "Odontología general" : o.Especialidad,
            Telefono = o.Telefono,
            Email = email,
            PasswordHash = hasher.Hash(o.Password)
        });
        logger.LogInformation("Seed: odontólogo {Email}.", email);
    }

    private static async Task SembrarPacienteAsync(
        DentalCareDbContext ctx, IPasswordHasher hasher, SeedPaciente p, ILogger logger, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(p.Email) || string.IsNullOrWhiteSpace(p.Password))
            return;

        var email = p.Email.Trim().ToLowerInvariant();
        if (await ctx.Pacientes.AnyAsync(x => x.Email == email, ct))
            return; // ya existe: no se duplica

        ctx.Pacientes.Add(new Paciente
        {
            Nombre = p.Nombre,
            Apellido = p.Apellido,
            Dni = p.Dni,
            FechaNacimiento = p.FechaNacimiento,
            Telefono = p.Telefono,
            Email = email,
            ObraSocial = p.ObraSocial,
            NumeroAfiliado = p.NumeroAfiliado,
            PasswordHash = hasher.Hash(p.Password)
        });
        logger.LogInformation("Seed: paciente {Email}.", email);
    }

    private static async Task SembrarConsultorioAsync(
        DentalCareDbContext ctx, SeedConsultorio c, ILogger logger, CancellationToken ct)
    {
        // El consultorio no tiene clave natural de texto: se identifica por piso + número (índice único).
        if (await ctx.Consultorios.AnyAsync(x => x.Piso == c.Piso && x.Numero == c.Numero, ct))
            return; // ya existe: no se duplica

        ctx.Consultorios.Add(new Consultorio
        {
            Numero = c.Numero,
            Piso = c.Piso,
            Nombre = c.Nombre,
            Equipamiento = c.Equipamiento
        });
        logger.LogInformation("Seed: consultorio {Piso}-{Numero}.", c.Piso, c.Numero);
    }

    private static async Task SembrarTratamientoAsync(
        DentalCareDbContext ctx, SeedTratamiento t, ILogger logger, CancellationToken ct)
    {
        var codigo = t.Codigo.Trim();
        if (string.IsNullOrWhiteSpace(codigo))
            return;

        if (await ctx.Tratamientos.AnyAsync(x => x.Codigo == codigo, ct))
            return; // ya existe: no se duplica

        ctx.Tratamientos.Add(new Tratamiento
        {
            Codigo = codigo,
            Nombre = t.Nombre,
            Descripcion = t.Descripcion,
            Costo = t.Costo,
            DuracionEstimada = t.DuracionEstimada
        });
        logger.LogInformation("Seed: tratamiento {Codigo}.", codigo);
    }
}
