using DentalCare.Application.Interfaces;
using DentalCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DentalCare.Infrastructure.Data;

/// <summary>
/// Aplica las migrations pendientes (si Database:MigrateOnStartup = true) y crea el primer
/// odontólogo para poder iniciar sesión, tomando email y contraseña de configuración
/// (Seed:Odontologo:Email / Seed:Odontologo:Password). Si no hay contraseña configurada no crea nada.
/// </summary>
public static class DataSeeder
{
    public static async Task InicializarAsync(IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;
        var context = sp.GetRequiredService<DentalCareDbContext>();
        var config = sp.GetRequiredService<IConfiguration>();
        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(DataSeeder));

        if (config.GetValue<bool>("Database:MigrateOnStartup") && context.Database.IsRelational())
        {
            logger.LogInformation("Aplicando migrations pendientes...");
            await context.Database.MigrateAsync(ct);
        }

        var email = config["Seed:Odontologo:Email"];
        var password = config["Seed:Odontologo:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return;

        if (await context.Odontologos.AnyAsync(ct))
            return;

        var hasher = sp.GetRequiredService<IPasswordHasher>();
        context.Odontologos.Add(new Odontologo
        {
            Nombre = config["Seed:Odontologo:Nombre"] ?? "Administrador",
            Apellido = config["Seed:Odontologo:Apellido"] ?? "DentalCare",
            Dni = config["Seed:Odontologo:Dni"] ?? "00000000",
            Matricula = config["Seed:Odontologo:Matricula"] ?? "MP-0000",
            Especialidad = config["Seed:Odontologo:Especialidad"] ?? "Odontología general",
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = hasher.Hash(password)
        });
        await context.SaveChangesAsync(ct);
        logger.LogInformation("Se creó el odontólogo inicial {Email}.", email);
    }
}
