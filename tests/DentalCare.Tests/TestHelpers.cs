using DentalCare.Application.DTOs;
using DentalCare.Application.Interfaces;
using DentalCare.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DentalCare.Tests;

/// <summary>Base SQLite en memoria: misma configuración de EF que en producción, sin necesitar MySQL.</summary>
public sealed class SqliteDb : IDisposable
{
    private readonly SqliteConnection _connection;

    public SqliteDb()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        using var ctx = CreateContext();
        ctx.Database.EnsureCreated();
    }

    public DentalCareDbContext CreateContext()
        => new(new DbContextOptionsBuilder<DentalCareDbContext>().UseSqlite(_connection).Options);

    public void Dispose() => _connection.Dispose();
}

/// <summary>Reemplazo del servicio externo de feriados.</summary>
public class FakeFeriadoService : IFeriadoService
{
    public List<FeriadoDto> Feriados { get; } = new();

    public Task<IReadOnlyList<FeriadoDto>> GetFeriadosAsync(int anio, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<FeriadoDto>>(Feriados.Where(f => f.Fecha.Year == anio).ToList());

    public Task<FeriadoDto?> GetFeriadoAsync(DateOnly fecha, CancellationToken ct = default)
        => Task.FromResult(Feriados.FirstOrDefault(f => f.Fecha == fecha));
}
