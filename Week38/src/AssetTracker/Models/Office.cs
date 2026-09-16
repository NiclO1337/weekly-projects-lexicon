namespace AssetTracker.Models;

internal sealed record Office(string Name, string Country, CurrencyCode Currency)
{
    internal static readonly Office Germany = new("Germany", "Germany", CurrencyCode.EUR);
    internal static readonly Office Sweden = new("Sweden", "Sweden", CurrencyCode.SEK);
    internal static readonly Office Usa = new("USA", "USA", CurrencyCode.USD);
    internal static readonly Office Turkey = new("Turkey", "Turkey", CurrencyCode.TRY);
}
