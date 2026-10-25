using System.Net.Http.Json;
using System.Text.Json.Serialization;
using DentalCare.Application.DTOs;
using DentalCare.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DentalCare.Infrastructure.ExternalServices;

public class FeriadoOptions
{
    public const string Section = "Feriados";

    public string BaseUrl { get; set; } = "https://date.nager.at/";
    public string CodigoPais { get; set; } = "AR";
    public int TimeoutSegundos { get; set; } = 5;
}

/// <summary>
/// Cliente tipado (HttpClient Factory) de la API pública Nager.Date que devuelve los feriados nacionales.
/// Se usa para no permitir reservar turnos en feriados y en el endpoint GET /api/feriados/{anio}.
/// </summary>
public class FeriadoService : IFeriadoService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly FeriadoOptions _options;
    private readonly ILogger<FeriadoService> _logger;

    public FeriadoService(HttpClient httpClient, IMemoryCache cache, IOptions<FeriadoOptions> options, ILogger<FeriadoService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<FeriadoDto>> GetFeriadosAsync(int anio, CancellationToken ct = default)
    {
        var cacheKey = $"feriados-{_options.CodigoPais}-{anio}";
        if (_cache.TryGetValue(cacheKey, out IReadOnlyList<FeriadoDto>? cacheados) && cacheados is not null)
            return cacheados;

        try
        {
            var respuesta = await _httpClient.GetFromJsonAsync<List<NagerFeriado>>(
                $"api/v3/PublicHolidays/{anio}/{_options.CodigoPais}", ct) ?? new();

            var feriados = respuesta
                .Select(f => new FeriadoDto(f.Date, f.Name, f.LocalName))
                .OrderBy(f => f.Fecha)
                .ToList();

            _cache.Set(cacheKey, (IReadOnlyList<FeriadoDto>)feriados, TimeSpan.FromHours(12));
            return feriados;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
        {
            // Si el servicio externo no responde no se bloquea la reserva de turnos.
            _logger.LogWarning(ex, "No se pudieron obtener los feriados de {Anio} desde Nager.Date.", anio);
            return Array.Empty<FeriadoDto>();
        }
    }

    public async Task<FeriadoDto?> GetFeriadoAsync(DateOnly fecha, CancellationToken ct = default)
    {
        var feriados = await GetFeriadosAsync(fecha.Year, ct);
        return feriados.FirstOrDefault(f => f.Fecha == fecha);
    }

    private sealed class NagerFeriado
    {
        [JsonPropertyName("date")] public DateOnly Date { get; set; }
        [JsonPropertyName("localName")] public string LocalName { get; set; } = string.Empty;
        [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    }
}
