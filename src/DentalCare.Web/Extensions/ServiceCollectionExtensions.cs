using System.Text;
using DentalCare.Infrastructure.Data;
using DentalCare.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace DentalCare.Web.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Autenticación JWT Bearer. El secret se lee de configuración: en Azure llega por la variable
    /// de ambiente Jwt__Key, que a su vez es una referencia a un secreto de Azure Key Vault.
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwt = configuration.GetSection(JwtOptions.Section).Get<JwtOptions>() ?? new JwtOptions();

        if (string.IsNullOrWhiteSpace(jwt.Key))
            throw new InvalidOperationException("Falta el secret 'Jwt:Key' (usar variable de ambiente Jwt__Key).");
        if (Encoding.UTF8.GetByteCount(jwt.Key) < 32)
            throw new InvalidOperationException("El secret 'Jwt:Key' debe tener al menos 32 bytes.");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                    ClockSkew = TimeSpan.FromMinutes(1)
                };

                // Los tokens de restablecimiento de contraseña no sirven para autenticarse.
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = ctx =>
                    {
                        if (ctx.Principal?.HasClaim(c => c.Type == "proposito") == true)
                            ctx.Fail("Token no válido para autenticación.");
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();
        return services;
    }

    /// <summary>Swagger con el botón "Authorize" para probar los endpoints con el token.</summary>
    public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "DentalCare API",
                Version = "v1",
                Description = "Gestión de turnos de un consultorio odontológico (TPI – UTN FRRo)."
            });

            var esquema = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Pegar el token obtenido en POST /api/auth/login",
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            };
            c.MapType<TimeOnly>(() => new OpenApiSchema { Type = "string", Format = "time", Example = new Microsoft.OpenApi.Any.OpenApiString("10:30") });
            c.AddSecurityDefinition("Bearer", esquema);
            c.AddSecurityRequirement(new OpenApiSecurityRequirement { [esquema] = Array.Empty<string>() });

            var xml = Path.Combine(AppContext.BaseDirectory, "DentalCare.Web.xml");
            if (File.Exists(xml)) c.IncludeXmlComments(xml);
        });
        return services;
    }

    public static IServiceCollection AddDentalCareHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks().AddDbContextCheck<DentalCareDbContext>("database");
        return services;
    }
}
