using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DentalCare.Web.Extensions;

/// <summary>Permite enviar la hora como "10:30" además de "10:30:00".</summary>
public class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
{
    private static readonly string[] Formatos = { "HH:mm", "HH:mm:ss", "H:mm" };

    public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var texto = reader.GetString();
        if (TimeOnly.TryParseExact(texto, Formatos, CultureInfo.InvariantCulture, DateTimeStyles.None, out var hora))
            return hora;
        throw new JsonException($"Hora inválida '{texto}'. Usar el formato HH:mm.");
    }

    public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString("HH:mm", CultureInfo.InvariantCulture));
}
