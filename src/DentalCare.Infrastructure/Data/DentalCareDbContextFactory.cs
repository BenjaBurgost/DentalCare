using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DentalCare.Infrastructure.Data;

/// <summary>
/// Usada solo por las herramientas de EF (dotnet ef migrations add ...).
/// Permite generar migrations sin tener MySQL levantado: no abre ninguna conexión.
/// </summary>
public class DentalCareDbContextFactory : IDesignTimeDbContextFactory<DentalCareDbContext>
{
    public DentalCareDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Server=localhost;Port=3306;Database=dentalcare;User=root;Password=changeme;";

        var options = new DbContextOptionsBuilder<DentalCareDbContext>()
            .UseMySql(connectionString, ServerVersion.Create(8, 0, 36, Pomelo.EntityFrameworkCore.MySql.Infrastructure.ServerType.MySql))
            .Options;

        return new DentalCareDbContext(options);
    }
}
