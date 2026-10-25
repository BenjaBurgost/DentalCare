using DentalCare.Application.Common;
using DentalCare.Application.DTOs;
using DentalCare.Application.Services;
using DentalCare.Domain.Enums;
using DentalCare.Domain.Exceptions;
using DentalCare.Infrastructure.Data;
using DentalCare.Infrastructure.Repositories;
using DentalCare.Infrastructure.Security;
using Microsoft.Extensions.Options;

namespace DentalCare.Tests;

public class AuthAndCrudServiceTests : IDisposable
{
    private readonly SqliteDb _db = new();
    private readonly DentalCareDbContext _ctx;
    private readonly PacienteService _pacientes;
    private readonly OdontologoService _odontologos;
    private readonly AuthService _auth;

    public AuthAndCrudServiceTests()
    {
        _ctx = _db.CreateContext();
        var pacienteRepo = new PacienteRepository(_ctx);
        var odontologoRepo = new OdontologoRepository(_ctx);
        var turnoRepo = new TurnoRepository(_ctx);
        var hasher = new PasswordHasher();
        var jwt = new JwtTokenGenerator(Options.Create(new JwtOptions { Key = new string('k', 40) }), TimeProvider.System);

        _pacientes = new PacienteService(pacienteRepo, odontologoRepo, turnoRepo, hasher);
        _odontologos = new OdontologoService(odontologoRepo, pacienteRepo, turnoRepo, hasher);
        _auth = new AuthService(pacienteRepo, odontologoRepo, _pacientes, hasher, jwt);
    }

    private static PacienteCreateDto Paciente(string dni = "30111222", string email = "ana@mail.com") => new()
    {
        Nombre = "Ana", Apellido = "Pérez", Dni = dni, FechaNacimiento = new DateOnly(1990, 1, 1),
        Email = email, Password = "Secreta123"
    };

    [Fact]
    public async Task Registro_YLogin_DevuelvenTokenConRolPaciente()
    {
        var registro = await _auth.RegisterAsync(Paciente());
        Assert.Equal(Roles.Paciente, registro.Rol);

        var login = await _auth.LoginAsync(new LoginDto { Email = "ANA@mail.com", Password = "Secreta123" });
        Assert.Equal(registro.UsuarioId, login.UsuarioId);
        Assert.False(string.IsNullOrWhiteSpace(login.Token));
    }

    [Fact]
    public async Task Login_ConPasswordIncorrecta_Falla()
    {
        await _auth.RegisterAsync(Paciente());
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _auth.LoginAsync(new LoginDto { Email = "ana@mail.com", Password = "otra" }));
    }

    [Fact]
    public async Task Password_SeGuardaHasheada()
    {
        var registro = await _auth.RegisterAsync(Paciente());
        var paciente = await _ctx.Pacientes.FindAsync(registro.UsuarioId);
        Assert.NotEqual("Secreta123", paciente!.PasswordHash);
    }

    [Fact]
    public async Task Registro_ConDniOEmailDuplicado_Falla()
    {
        await _auth.RegisterAsync(Paciente());
        await Assert.ThrowsAsync<DomainException>(() => _auth.RegisterAsync(Paciente(email: "otra@mail.com")));
        await Assert.ThrowsAsync<DomainException>(() => _auth.RegisterAsync(Paciente(dni: "40999888")));
    }

    [Fact]
    public async Task Email_NoPuedeRepetirseEntrePacientesYOdontologos()
    {
        await _auth.RegisterAsync(Paciente());
        await Assert.ThrowsAsync<DomainException>(() => _odontologos.CreateAsync(new OdontologoCreateDto
        {
            Nombre = "Juan", Apellido = "Díaz", Dni = "20333444", Matricula = "MP-1", Especialidad = "General",
            Email = "ana@mail.com", Password = "Secreta123"
        }));
    }

    [Fact]
    public async Task ResetPassword_FuncionaUnaSolaVez()
    {
        await _auth.RegisterAsync(Paciente());
        var token = await _auth.ForgotPasswordAsync(new ForgotPasswordDto { Email = "ana@mail.com" });
        Assert.NotNull(token);

        await _auth.ResetPasswordAsync(new ResetPasswordDto { Token = token!, NuevaPassword = "NuevaClave99" });
        var login = await _auth.LoginAsync(new LoginDto { Email = "ana@mail.com", Password = "NuevaClave99" });
        Assert.Equal(Roles.Paciente, login.Rol);

        await Assert.ThrowsAsync<DomainException>(() =>
            _auth.ResetPasswordAsync(new ResetPasswordDto { Token = token!, NuevaPassword = "OtraClave99" }));
    }

    [Fact]
    public async Task ForgotPassword_EmailInexistente_NoRevelaNada()
        => Assert.Null(await _auth.ForgotPasswordAsync(new ForgotPasswordDto { Email = "nadie@mail.com" }));

    [Fact]
    public async Task Paciente_SoloPuedeModificarseASiMismo()
    {
        var ana = await _pacientes.CreateAsync(Paciente());
        var luis = await _pacientes.CreateAsync(Paciente("30999000", "luis@mail.com"));

        var cambios = new PacienteUpdateDto
        {
            Nombre = "Ana María", Apellido = "Pérez", Dni = ana.Dni, FechaNacimiento = ana.FechaNacimiento, Email = ana.Email
        };

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _pacientes.UpdateAsync(ana.Id, cambios, new UsuarioActual(luis.Id, Roles.Paciente)));

        var actualizado = await _pacientes.UpdateAsync(ana.Id, cambios, new UsuarioActual(ana.Id, Roles.Paciente));
        Assert.Equal("Ana María", actualizado.Nombre);
    }

    public void Dispose()
    {
        _ctx.Dispose();
        _db.Dispose();
    }
}
