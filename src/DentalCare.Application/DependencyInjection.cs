using DentalCare.Application.Interfaces;
using DentalCare.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DentalCare.Application;

public static class DependencyInjection
{
    /// <summary>Registra los servicios de negocio de la capa Application.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPacienteService, PacienteService>();
        services.AddScoped<IOdontologoService, OdontologoService>();
        services.AddScoped<IConsultorioService, ConsultorioService>();
        services.AddScoped<ITratamientoService, TratamientoService>();
        services.AddScoped<ITurnoService, TurnoService>();

        return services;
    }
}
