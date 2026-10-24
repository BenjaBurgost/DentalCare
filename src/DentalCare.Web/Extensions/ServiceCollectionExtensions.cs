using DentalCare.Application.Interfaces;
using DentalCare.Application.Services;

namespace DentalCare.Web.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>Registra los servicios de la capa Application.</summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPacienteService, PacienteService>();
        services.AddScoped<IOdontologoService, OdontologoService>();
        services.AddScoped<IConsultorioService, ConsultorioService>();
        services.AddScoped<ITratamientoService, TratamientoService>();
        services.AddScoped<ITurnoService, TurnoService>();

        return services;
    }
}
