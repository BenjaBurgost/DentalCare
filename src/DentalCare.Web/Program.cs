using System.Text.Json.Serialization;
using DentalCare.Application;
using DentalCare.Infrastructure;
using DentalCare.Infrastructure.Data;
using DentalCare.Web.Extensions;
using DentalCare.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ---------- Servicios ----------
builder.Services
    .AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        o.JsonSerializerOptions.Converters.Add(new TimeOnlyJsonConverter());
    });

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddSwaggerWithJwt();

// Infraestructura: DbContext (MySQL), repositorios, JWT, hashing y cliente HTTP de feriados
builder.Services.AddInfrastructure(builder.Configuration);

// Aplicación: servicios de negocio
builder.Services.AddApplication();

// Autenticación JWT. El secret se lee de configuración / variable de ambiente Jwt__Key (Key Vault en Azure).
builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddDentalCareHealthChecks();

var app = builder.Build();

// Migrations + odontólogo inicial (según configuración)
await DataSeeder.InicializarAsync(app.Services);

// ---------- Pipeline HTTP ----------
app.UseExceptionHandler();
app.UseStatusCodePages();

// Swagger queda disponible también en Azure para la defensa del TPI (Swagger:Enabled = false lo apaga).
if (app.Environment.IsDevelopment() || app.Configuration.GetValue("Swagger:Enabled", true))
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.DocumentTitle = "DentalCare API");
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.Run();

// Necesario para los tests de integración (WebApplicationFactory<Program>).
public partial class Program;
