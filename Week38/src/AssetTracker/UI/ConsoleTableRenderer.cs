using AssetTracker.Models;
using AssetTracker.Services;

namespace AssetTracker.UI;

internal static class ConsoleTableRenderer
{
    internal static void RenderAssets(IReadOnlyList<Asset> assets)
    {
        const int extraPadding = 3;

        int idWidth = Math.Max("id".Length, assets.Max(a => a.Id.ToString().Length)) + extraPadding;
        int typeWidth = Math.Max("Type".Length, assets.Max(a => a.GetCategoryLabel().Length)) + extraPadding;
        int brandWidth = Math.Max("Brand".Length, assets.Max(a => a.Brand.Length)) + extraPadding;
        int modelWidth = Math.Max("Model".Length, assets.Max(a => a.Model.Length)) + extraPadding;
        int dateWidth = Math.Max("Purchase Date".Length, assets.Max(a => a.PurchaseDate.ToString("yyyy-MM-dd").Length)) + extraPadding;
        int priceWidth = Math.Max("Price (EUR)".Length, assets.Max(a => FormatHelpers.FormatPrice(a.PriceEur).Length)) + extraPadding;
        int officeWidth = Math.Max("Office".Length, assets.Max(a => a.Office.Name.Length)) + extraPadding;
        int statusWidth = Math.Max("EOL Status".Length, assets.Max(a => a.GetEndOfLifeStatusLabel().Length));

        string header = $"{"Id".PadRight(idWidth)}{"Type".PadRight(typeWidth)}{"Brand".PadRight(brandWidth)}" +
            $"{"Model".PadRight(modelWidth)}{"Purchase Date".PadRight(dateWidth)}{"Price (EUR)".PadRight(priceWidth)}" +
            $"{"Office".PadRight(officeWidth)}{"EOL Status".PadRight(statusWidth)}";

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

            Console.WriteLine($"{FormatHelpers.FormatPrice(asset.PriceEur).PadRight(priceWidth)}" +
                $"{asset.Office.Name.PadRight(officeWidth)}{asset.GetEndOfLifeStatusLabel().PadRight(statusWidth)}");
        }

        Console.WriteLine(new string('-', header.Length));
    }
}
