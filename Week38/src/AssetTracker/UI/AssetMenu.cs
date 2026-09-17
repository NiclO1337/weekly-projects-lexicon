using AssetTracker.Models;
using AssetTracker.Services;

namespace AssetTracker.UI;

internal static class AssetMenu
{
    private static readonly List<Office> Offices = [Office.Germany, Office.Sweden, Office.Usa, Office.Turkey];

    internal static void HandleAddAsset(AssetService assetService)
    {
        ConsoleHelpers.Heading("Add Asset");

        List<string> assetTypes = ["Computer", "Mobile Phone", "Tablet"];
        string assetType = ConsoleHelpers.SelectFromList(
            assetTypes,
            type => type,
            $"Select asset type (1 - {assetTypes.Count}): ",
            allowCancel: true);

        List<ComputerType> computerTypes = Enum.GetValues<ComputerType>().ToList();
        ComputerType? computerType = assetType == "Computer"
            ? ConsoleHelpers.SelectFromList(
                computerTypes,
                t => t.ToString(),
                $"Select computer type (1 - {computerTypes.Count}): ",
                allowCancel: true)
            : null;

        string brand = ConsoleHelpers.ValidateInput("Brand: ", allowCancel: true);
        string model = ConsoleHelpers.ValidateInput("Model: ", allowCancel: true);

        DateTime purchaseDate = ConsoleHelpers.ValidateInput(
            "Purchase date (yyyy-MM-dd): ",
            FormatHelpers.ValidateDate(),
            "Invalid date. Use format yyyy-MM-dd and don't pick a future date.",
            allowCancel: true);

        decimal priceEur = ConsoleHelpers.ValidateInput(
            "Price in EUR: ",
            FormatHelpers.ValidatePositiveDecimal(),
            "Invalid price.",
            allowCancel: true);

        Office office = ConsoleHelpers.SelectFromList(
            Offices,
            o => $"{o.Name} ({o.Currency})",
            $"Select office (1 - {Offices.Count}): ",
            allowCancel: true);

        Asset asset = assetType switch
        {
            "Computer" => new Computer(0, brand, model, purchaseDate, priceEur, office, computerType!.Value),
            "Mobile Phone" => new MobilePhone(0, brand, model, purchaseDate, priceEur, office),
            "Tablet" => new Tablet(0, brand, model, purchaseDate, priceEur, office),
            _ => throw new InvalidOperationException("Unexpected asset type."),
        };

        assetService.AddAsset(asset);

        ConsoleHelpers.DisplaySuccessMessage($"A {assetType.ToLower()} added to {office.Name} office ({brand} - {model}).");
    }

    internal static void HandleViewAssets(AssetService assetService)
    {
        AssetSortMode sortMode = AssetSortMode.Office;
        int currentPage = 1;

        while (true)
        {
            ConsoleHelpers.Heading("View Assets");

            AssetsPage page = assetService.GetAssetsPage(sortMode, currentPage);
            currentPage = page.PageNumber;

            if (page.Items.Count == 0)
            {
                ConsoleHelpers.DisplayWarningMessage("No assets found.");
                return;
            }

            ConsoleTableRenderer.RenderAssets(page.Items);
            Console.WriteLine($"\nPage {page.PageNumber} of {page.TotalPages} (sorted by {GetSortModeLabel(sortMode).ToLower()})\n");

            List<string> menuItems = ["Previous Page", "Next Page\n", "Sort: Office", "Sort: Asset Type", "Sort: End of Life", "Back to Main Menu"];
            string choice = ConsoleHelpers.SelectFromList(menuItems, item => item, $"Select option (1 - {menuItems.Count}): ");

            switch (choice)
            {
                case "Previous Page":
                    if (currentPage <= 1)
                    {
                        ConsoleHelpers.DisplayWarningMessage("Already on the first page.");
                    }
                    else
                    {
                        currentPage--;
                    }
                    break;
                case "Next Page\n":
                    if (currentPage >= page.TotalPages)
                    {
                        ConsoleHelpers.DisplayWarningMessage("Already on the last page.");
                    }
                    else
                    {
                        currentPage++;
                    }
                    break;
                case "Sort: Office (default)":
                    sortMode = AssetSortMode.Office;
                    currentPage = 1;
                    break;
                case "Sort: Asset Type":
                    sortMode = AssetSortMode.AssetType;
                    currentPage = 1;
                    break;
                case "Sort: End of Life":
                    sortMode = AssetSortMode.EndOfLife;
                    currentPage = 1;
                    break;
                case "Back to Main Menu":
                    OutputTracker.HasWritten = false;  return;
            }
        }
    }

    private static string GetSortModeLabel(AssetSortMode mode)
    {
        return mode switch
        {
            AssetSortMode.AssetType => "Asset Type",
            AssetSortMode.EndOfLife => "End of Life",
            _ => "Office",
        };
    }
}
