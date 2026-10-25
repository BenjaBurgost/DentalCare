using DentalCare.Application.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace DentalCare.Infrastructure.Security;

/// <summary>Hash PBKDF2 (con salt e iteraciones) usando el hasher de ASP.NET Core Identity.</summary>
public class PasswordHasher : IPasswordHasher
{
    private static readonly PasswordHasher<object> Hasher = new();
    private static readonly object Usuario = new();

    public string Hash(string password) => Hasher.HashPassword(Usuario, password);

    public bool Verify(string hash, string password)
    {
        if (string.IsNullOrEmpty(hash)) return false;
        return Hasher.VerifyHashedPassword(Usuario, hash, password) != PasswordVerificationResult.Failed;
    }
}
