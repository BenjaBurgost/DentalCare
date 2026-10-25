using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DentalCare.Application.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DentalCare.Infrastructure.Security;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private const string Proposito = "proposito";
    private const string PropositoReset = "reset-password";
    private const string ClaimSello = "sello";

    private readonly JwtOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly SymmetricSecurityKey _key;

    public JwtTokenGenerator(IOptions<JwtOptions> options, TimeProvider timeProvider)
    {
        _options = options.Value;
        _timeProvider = timeProvider;
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
    }

    public (string Token, DateTime Expira) GenerarToken(int usuarioId, string nombre, string email, string rol)
    {
        var expira = _timeProvider.GetUtcNow().UtcDateTime.AddMinutes(_options.ExpirationMinutes);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuarioId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.Name, nombre),
            new Claim(ClaimTypes.Role, rol)
        };
        return (Escribir(claims, expira), expira);
    }

    public string GenerarResetToken(int usuarioId, string rol, string passwordHashActual)
    {
        var expira = _timeProvider.GetUtcNow().UtcDateTime.AddMinutes(_options.ResetExpirationMinutes);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuarioId.ToString()),
            new Claim(ClaimTypes.Role, rol),
            new Claim(Proposito, PropositoReset),
            new Claim(ClaimSello, CalcularSello(passwordHashActual))
        };
        return Escribir(claims, expira);
    }

    public ResetTokenInfo? ValidarResetToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
            var principal = handler.ValidateToken(token, ParametrosValidacion(), out _);

            if (principal.FindFirst(Proposito)?.Value != PropositoReset) return null;

            var id = int.Parse(principal.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);
            var rol = principal.FindFirst(ClaimTypes.Role)?.Value
                      ?? principal.FindFirst("role")?.Value
                      ?? string.Empty;
            var sello = principal.FindFirst(ClaimSello)?.Value ?? string.Empty;
            return new ResetTokenInfo(id, rol, sello);
        }
        catch (Exception ex) when (ex is SecurityTokenException or ArgumentException or FormatException)
        {
            return null;
        }
    }

    public string CalcularSello(string passwordHash)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(passwordHash)))[..16];

    private TokenValidationParameters ParametrosValidacion() => new()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = _options.Issuer,
        ValidAudience = _options.Audience,
        IssuerSigningKey = _key,
        LifetimeValidator = (_, expires, _, _) => expires > _timeProvider.GetUtcNow().UtcDateTime
    };

    private string Escribir(IEnumerable<Claim> claims, DateTime expira)
    {
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: _timeProvider.GetUtcNow().UtcDateTime,
            expires: expira,
            signingCredentials: new SigningCredentials(_key, SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
