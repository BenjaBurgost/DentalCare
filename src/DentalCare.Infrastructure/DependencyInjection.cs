using DentalCare.Domain.Interfaces;
using DentalCare.Infrastructure.Data;
using DentalCare.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DentalCare.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        // Versión de servidor fija: evita que EF abra una conexión al arrancar o al generar migrations.
        var serverVersion = ServerVersion.Create(8, 0, 36, Pomelo.EntityFrameworkCore.MySql.Infrastructure.ServerType.MySql);

        services.AddDbContext<DentalCareDbContext>(options =>
            options.UseMySql(connectionString, serverVersion));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IPacienteRepository, PacienteRepository>();
        services.AddScoped<IOdontologoRepository, OdontologoRepository>();
        services.AddScoped<IConsultorioRepository, ConsultorioRepository>();
        services.AddScoped<ITratamientoRepository, TratamientoRepository>();
        services.AddScoped<ITurnoRepository, TurnoRepository>();

        return services;
    }
}
