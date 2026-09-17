using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AssetTracker.Models;

/// <summary>
/// Persists PurchaseDate as a plain yyyy-MM-dd string instead of a full ISO 8601
/// timestamp, matching how dates are entered/displayed everywhere else in the app.
/// </summary>
internal sealed class DateOnlyJsonConverter : JsonConverter<DateTime>
{
    private const string Format = "yyyy-MM-dd";

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTime.ParseExact(reader.GetString()!, Format, CultureInfo.InvariantCulture);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(Format, CultureInfo.InvariantCulture));
    }
}
