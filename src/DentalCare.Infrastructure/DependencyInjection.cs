using DentalCare.Application.Interfaces;
using DentalCare.Domain.Interfaces;
using DentalCare.Infrastructure.Data;
using DentalCare.Infrastructure.ExternalServices;
using DentalCare.Infrastructure.Repositories;
using DentalCare.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DentalCare.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'DefaultConnection'.");

        // Versión de servidor fija: evita que EF abra una conexión al arrancar o al generar migrations.
        var serverVersion = ServerVersion.Create(8, 0, 36, Pomelo.EntityFrameworkCore.MySql.Infrastructure.ServerType.MySql);

        services.AddDbContext<DentalCareDbContext>(options =>
            options.UseMySql(connectionString, serverVersion, mysql => mysql.EnableRetryOnFailure(3)));

        // Repositorios: genérico + específicos
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IPacienteRepository, PacienteRepository>();
        services.AddScoped<IOdontologoRepository, OdontologoRepository>();
        services.AddScoped<IConsultorioRepository, ConsultorioRepository>();
        services.AddScoped<ITratamientoRepository, TratamientoRepository>();
        services.AddScoped<ITurnoRepository, TurnoRepository>();

        // Seguridad
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.Section));
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        // Servicio de terceros (feriados) mediante HttpClient Factory con cliente tipado
        services.AddMemoryCache();
        services.Configure<FeriadoOptions>(configuration.GetSection(FeriadoOptions.Section));
        services.AddHttpClient<IFeriadoService, FeriadoService>((sp, client) =>
        {
            var opciones = sp.GetRequiredService<IOptions<FeriadoOptions>>().Value;
            client.BaseAddress = new Uri(opciones.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(opciones.TimeoutSegundos);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        return services;
    }
}
