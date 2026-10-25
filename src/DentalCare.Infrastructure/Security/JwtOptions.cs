namespace DentalCare.Infrastructure.Security;

public class JwtOptions
{
    public const string Section = "Jwt";

    /// <summary>Secret de firma. En Azure viene de Key Vault mediante la variable de ambiente Jwt__Key.</summary>
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = "DentalCare.Api";
    public string Audience { get; set; } = "DentalCare.Clients";
    public int ExpirationMinutes { get; set; } = 120;
    public int ResetExpirationMinutes { get; set; } = 30;
}
