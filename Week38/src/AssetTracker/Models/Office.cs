using System.Text.Json.Serialization;
using AssetTracker.Exceptions;

namespace AssetTracker.Models;

[JsonConverter(typeof(OfficeJsonConverter))]
internal sealed record Office(string Name, string Country, CurrencyCode Currency)
{
    internal static readonly Office Germany = new("Germany", "Germany", CurrencyCode.EUR);
    internal static readonly Office Sweden = new("Sweden", "Sweden", CurrencyCode.SEK);
    internal static readonly Office Usa = new("USA", "USA", CurrencyCode.USD);
    internal static readonly Office Turkey = new("Turkey", "Turkey", CurrencyCode.TRY);

    internal static readonly IReadOnlyList<Office> All = [Germany, Sweden, Usa, Turkey];

    internal static Office FromName(string name)
    {
        return All.FirstOrDefault(o => o.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidAssetDataException($"Unknown office '{name}'.");
    }
}
