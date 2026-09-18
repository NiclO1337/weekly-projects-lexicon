using AssetTracker.Models;
using AssetTracker.Services;

namespace AssetTracker.UI;

internal static class ConsoleTableRenderer
{
    internal static void RenderAssets(IReadOnlyList<Asset> assets, ICurrencyProvider currencyProvider)
    {
        const int bigPadding = 3;
        const int mediumPadding = 2;
        const int smallPadding = 1;

        int idWidth = Math.Max("id".Length, assets.Max(a => a.Id.ToString().Length)) + mediumPadding;
        int typeWidth = Math.Max("Type".Length, assets.Max(a => a.GetCategoryLabel().Length)) + bigPadding;
        int brandWidth = Math.Max("Brand".Length, assets.Max(a => a.Brand.Length)) + bigPadding;
        int modelWidth = Math.Max("Model".Length, assets.Max(a => a.Model.Length)) + bigPadding;
        int dateWidth = Math.Max("Purchased".Length, assets.Max(a => a.PurchaseDate.ToString("yyyy-MM-dd").Length)) + mediumPadding;
        int priceWidth = Math.Max("Price".Length, assets.Max(a => FormatHelpers.FormatPrice(a.PriceEur).Length)) + mediumPadding;
        int officeWidth = Math.Max("Office".Length, assets.Max(a => a.Office.Name.Length)) + mediumPadding;
        int localValueWidth = Math.Max("Local".Length, assets.Max(a => FormatHelpers.FormatPrice(AssetService.GetLocalPrice(a, currencyProvider)).Length)) + smallPadding;
        int currencyWidth = Math.Max("Code".Length, assets.Max(a => a.Office.Currency.ToString().Length)) + mediumPadding;
        int statusWidth = Math.Max("EOL Status".Length, assets.Max(a => a.GetEndOfLifeStatusLabel().Length));

        string localValueHeaderCell = "Local".PadLeft(localValueWidth - smallPadding) + new string(' ', smallPadding);
        string priceHeaderCell = "Price".PadLeft(priceWidth - mediumPadding) + new string(' ', mediumPadding);

        string header = $"{"Id".PadRight(idWidth)}{"Type".PadRight(typeWidth)}{"Brand".PadRight(brandWidth)}" +
            $"{"Model".PadRight(modelWidth)}{"Purchased".PadRight(dateWidth)}{priceHeaderCell}" +
            $"{"Office".PadRight(officeWidth)}{localValueHeaderCell}{"Code".PadRight(currencyWidth)}" +
            $"{"EOL Status".PadRight(statusWidth)}";

        Console.WriteLine(header);
        Console.WriteLine(new string('-', header.Length));

        foreach (Asset asset in assets)
        {
            Console.Write($"{asset.Id.ToString().PadRight(idWidth)}{asset.GetCategoryLabel().PadRight(typeWidth)}" +
                $"{asset.Brand.PadRight(brandWidth)}{asset.Model.PadRight(modelWidth)}");

            EndOfLifeStatus status = asset.GetEndOfLifeStatus();
            ConsoleColor? dateColor = status switch
            {
                EndOfLifeStatus.Red => ConsoleColor.Red,
                EndOfLifeStatus.Yellow => ConsoleColor.DarkYellow,
                EndOfLifeStatus.DarkRed => ConsoleColor.DarkRed,
                _ => null,
            };

            if (dateColor is not null)
            {
                Console.ForegroundColor = dateColor.Value;
            }

            Console.Write(asset.PurchaseDate.ToString("yyyy-MM-dd").PadRight(dateWidth));

            if (dateColor is not null)
            {
                Console.ResetColor();
            }

            string localValueCell = FormatHelpers.FormatPrice(AssetService.GetLocalPrice(asset, currencyProvider))
                .PadLeft(localValueWidth - smallPadding) + new string(' ', smallPadding);

            string priceCell = FormatHelpers.FormatPrice(asset.PriceEur).PadLeft(priceWidth - mediumPadding) + new string (' ', mediumPadding);

            Console.WriteLine($"{priceCell}" +
                $"{asset.Office.Name.PadRight(officeWidth)}" +
                $"{localValueCell}" +
                $"{asset.Office.Currency.ToString().PadRight(currencyWidth)}" +
                $"{asset.GetEndOfLifeStatusLabel().PadRight(statusWidth)}");
        }

        Console.WriteLine(new string('-', header.Length));
    }
}
