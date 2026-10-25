using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DentalCare.Application.DTOs;
using DentalCare.Application.Interfaces;
using DentalCare.Infrastructure.Data;
using DentalCare.Web.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DentalCare.Tests;

/// <summary>Levanta la API completa (controladores, JWT, filtros) sobre SQLite en memoria.</summary>
public class DentalCareApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public const string AdminEmail = "admin@test.com";
    public const string AdminPassword = "Admin1234!";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:DefaultConnection", "Server=no-usado");
        builder.UseSetting("Jwt:Key", "clave-de-tests-de-integracion-0123456789-abcdef");
        builder.UseSetting("Database:MigrateOnStartup", "false");
        builder.UseSetting("Seed:Odontologo:Email", AdminEmail);
        builder.UseSetting("Seed:Odontologo:Password", AdminPassword);

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<DentalCareDbContext>>();
            services.AddDbContext<DentalCareDbContext>(o => o.UseSqlite(_connection));

            services.RemoveAll<IFeriadoService>();
            services.AddSingleton<IFeriadoService, FakeFeriadoService>();

            using var scope = services.BuildServiceProvider().CreateScope();
            scope.ServiceProvider.GetRequiredService<DentalCareDbContext>().Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection.Dispose();
    }
}

public class ApiIntegrationTests : IClassFixture<DentalCareApiFactory>
{
    private readonly DentalCareApiFactory _factory;

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(), new TimeOnlyJsonConverter() }
    };

    public ApiIntegrationTests(DentalCareApiFactory factory) => _factory = factory;

    private async Task<HttpClient> ClienteAutenticadoAsync(string email, string password)
    {
        var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        login.EnsureSuccessStatusCode();
        var auth = await login.Content.ReadFromJsonAsync<AuthResponseDto>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);
        return client;
    }

    [Fact]
    public async Task SinToken_Devuelve401()
    {
        var response = await _factory.CreateClient().GetAsync("/api/turnos");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task LoginIncorrecto_Devuelve401()
    {
        var response = await _factory.CreateClient().PostAsJsonAsync("/api/auth/login",
            new { email = DentalCareApiFactory.AdminEmail, password = "mal" });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DatosInvalidos_Devuelve400()
    {
        var admin = await ClienteAutenticadoAsync(DentalCareApiFactory.AdminEmail, DentalCareApiFactory.AdminPassword);
        var response = await admin.PostAsJsonAsync("/api/consultorios", new { numero = 0, piso = 1 });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task FlujoCompleto_OdontologoCreaRecursos_PacienteReservaTurno()
    {
        var admin = await ClienteAutenticadoAsync(DentalCareApiFactory.AdminEmail, DentalCareApiFactory.AdminPassword);

        var consultorio = await (await admin.PostAsJsonAsync("/api/consultorios", new { numero = 10, piso = 1, nombre = "Box 10" }))
            .Content.ReadFromJsonAsync<ConsultorioDto>();
        var tratamiento = await (await admin.PostAsJsonAsync("/api/tratamientos", new { codigo = "LIM", nombre = "Limpieza", costo = 1000, duracionEstimada = 30 }))
            .Content.ReadFromJsonAsync<TratamientoDto>();
        var odontologos = await admin.GetFromJsonAsync<List<OdontologoDto>>("/api/odontologos");

        // Registro de paciente
        var anon = _factory.CreateClient();
        var registro = await anon.PostAsJsonAsync("/api/auth/register", new
        {
            nombre = "Ana", apellido = "Pérez", dni = "30111222", fechaNacimiento = "1990-05-01",
            email = "ana@test.com", password = "Secreta123"
        });
        Assert.Equal(HttpStatusCode.Created, registro.StatusCode);

        var paciente = await ClienteAutenticadoAsync("ana@test.com", "Secreta123");

        // El paciente no puede listar pacientes ni crear consultorios
        Assert.Equal(HttpStatusCode.Forbidden, (await paciente.GetAsync("/api/pacientes")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await paciente.PostAsJsonAsync("/api/consultorios", new { numero = 11, piso = 1 })).StatusCode);

        // Próximo martes dentro de al menos una semana
        var fecha = DateOnly.FromDateTime(DateTime.Today.AddDays(7));
        while (fecha.DayOfWeek != DayOfWeek.Tuesday) fecha = fecha.AddDays(1);

        var turnoBody = new
        {
            fecha = fecha.ToString("yyyy-MM-dd"), hora = "10:00", odontologoId = odontologos![0].Id,
            consultorioId = consultorio!.Id, tratamientoIds = new[] { tratamiento!.Id }
        };
        var creado = await paciente.PostAsJsonAsync("/api/turnos", turnoBody);
        Assert.Equal(HttpStatusCode.Created, creado.StatusCode);

        // Mismo horario -> 409
        var repetido = await admin.PostAsJsonAsync("/api/turnos", new
        {
            turnoBody.fecha, hora = "10:15", pacienteId = 1, turnoBody.odontologoId, turnoBody.consultorioId, turnoBody.tratamientoIds
        });
        Assert.Equal(HttpStatusCode.Conflict, repetido.StatusCode);

        var misTurnos = await paciente.GetFromJsonAsync<List<TurnoDto>>("/api/turnos", Json);
        var turno = Assert.Single(misTurnos!);
        Assert.Equal(new TimeOnly(10, 0), turno.Hora);

        var confirmar = await admin.PatchAsJsonAsync($"/api/turnos/{turno.Id}/estado", new { estado = "Confirmado" });
        Assert.Equal(HttpStatusCode.OK, confirmar.StatusCode);

        var cancelar = await paciente.PostAsync($"/api/turnos/{turno.Id}/cancelar", null);
        Assert.Equal(HttpStatusCode.OK, cancelar.StatusCode);
    }

    [Fact]
    public async Task Feriados_EsAnonimo()
    {
        var response = await _factory.CreateClient().GetAsync("/api/feriados/2026");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
