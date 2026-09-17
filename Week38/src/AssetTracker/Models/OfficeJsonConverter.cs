using System.Text.Json;
using System.Text.Json.Serialization;
using AssetTracker.Exceptions;

namespace AssetTracker.Models;

internal sealed class OfficeJsonConverter : JsonConverter<Office>
{
    public override Office Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string name = reader.GetString() ?? throw new InvalidAssetDataException("Office name cannot be null.");
        return Office.FromName(name);
    }

    public override void Write(Utf8JsonWriter writer, Office value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Name);
    }
}
