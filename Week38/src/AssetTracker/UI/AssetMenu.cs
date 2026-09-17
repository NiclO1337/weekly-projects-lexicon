using AssetTracker.Exceptions;
using AssetTracker.Models;
using AssetTracker.Services;

namespace AssetTracker.UI;

internal static class AssetMenu
{
    internal static void HandleAddAsset(AssetService assetService)
    {
        ConsoleHelpers.Heading("Add Asset");

        List<string> assetTypes = ["Computer", "Mobile Phone", "Tablet"];
        string assetType = ConsoleHelpers.SelectFromList(
            assetTypes,
            type => type,
            $"Select asset type (1 - {assetTypes.Count} or \"q\" to quit): ",
            allowCancel: true);

        List<ComputerType> computerTypes = Enum.GetValues<ComputerType>().ToList();
        ComputerType? computerType = assetType == "Computer"
            ? ConsoleHelpers.SelectFromList(
                computerTypes,
                t => t.ToString(),
                $"Select computer type (1 - {computerTypes.Count} or \"q\" to quit): ",
                allowCancel: true)
            : null;

        string brand = ConsoleHelpers.ValidateInput("Brand (or \"q\" to quit): ", allowCancel: true);
        string model = ConsoleHelpers.ValidateInput("Model (or \"q\" to quit): ", allowCancel: true);

        DateTime purchaseDate = ConsoleHelpers.ValidateInput(
            "Purchase date (yyyy-MM-dd) (or \"q\" to quit): ",
            FormatHelpers.ValidateDate(),
            "Invalid date. Use format yyyy-MM-dd and don't pick a future date.",
            allowCancel: true);

        decimal priceEur = ConsoleHelpers.ValidateInput(
            "Price in EUR (or \"q\" to quit): ",
            FormatHelpers.ValidatePositiveDecimal(),
            "Invalid price.",
            allowCancel: true);

        Office office = ConsoleHelpers.SelectFromList(
            Office.All,
            o => $"{o.Name} ({o.Currency})",
            $"Select office (1 - {Office.All.Count} or \"q\" to quit): ",
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

            ConsoleTableRenderer.RenderAssets(page.Items, assetService.CurrencyProvider);
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
                case "Sort: Office":
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

    internal static void HandleSearchAssets(AssetService assetService)
    {
        ConsoleHelpers.Heading("Search Assets");

        List<string> searchOptions = ["Search by Brand", "Search by Model"];
        string searchOption = ConsoleHelpers.SelectFromList(
            searchOptions,
            option => option,
            $"Select option (1 - {searchOptions.Count} or \"q\" to quit): ",
            allowCancel: true);

        string term = ConsoleHelpers.ValidateInput(
            searchOption == "Search by Brand" 
            ? "Brand (or \"q\" to quit): " 
            : "Model (or \"q\" to quit): ",
            allowCancel: true);

        IReadOnlyList<Asset> results = searchOption == "Search by Brand"
            ? assetService.SearchByBrand(term)
            : assetService.SearchByModel(term);

        if (results.Count == 0)
        {
            ConsoleHelpers.DisplayWarningMessage("No assets found.");
            return;
        }
        else
        {
            ConsoleHelpers.DisplaySuccessMessage(
                $"Found {results.Count} {FormatHelpers.Pluralize(results.Count, "asset", "assets")} that matched \"{term}\"");
        }

        Console.WriteLine();
        ConsoleTableRenderer.RenderAssets(results, assetService.CurrencyProvider);
    }

    internal static void HandleEditAssets(AssetService assetService)
    {
        ConsoleHelpers.Heading("Edit Assets");

        int? id = SelectAssetIdFromList(assetService, "edit");
        if (id is null)
        {
            return;
        }

        Asset asset;
        try
        {
            asset = assetService.GetById(id.Value);
        }
        catch (AssetNotFoundException ex)
        {
            ConsoleHelpers.DisplayErrorMessage(ex.Message);
            return;
        }

        while (true)
        {
            Console.WriteLine($"\nEditing: {asset.Brand} {asset.Model} (Id {asset.Id})");

            List<string> menuItems = ["Edit All Fields"];
            if (asset is Computer)
            {
                menuItems.Add("Change Computer Type");
            }
            menuItems.AddRange(["Change Asset Type", "Change Brand", "Change Model", "Change Purchase Date", "Change Price", "Change Office", "Back to Main Menu"]);

            string choice = ConsoleHelpers.SelectFromList(menuItems, item => item, $"Select option (1 - {menuItems.Count}): ");

            switch (choice)
            {
                case "Edit All Fields":
                    asset = ChangeAssetType(assetService, asset);
                    EditBrand(asset);
                    EditModel(asset);
                    EditPurchaseDate(asset);
                    EditPrice(asset);
                    EditOffice(asset);
                    ConsoleHelpers.DisplaySuccessMessage("Updated successfully.");
                    break;
                case "Change Asset Type":
                    Asset updatedAsset = ChangeAssetType(assetService, asset);
                    if (ReferenceEquals(updatedAsset, asset))
                    {
                        ConsoleHelpers.DisplayWarningMessage("Asset type unchanged.");
                    }
                    else
                    {
                        asset = updatedAsset;
                        ConsoleHelpers.DisplaySuccessMessage("Asset type updated successfully.");
                    }
                    break;
                case "Change Brand":
                    EditBrand(asset);
                    ConsoleHelpers.DisplaySuccessMessage("Brand updated successfully.");
                    break;
                case "Change Model":
                    EditModel(asset);
                    ConsoleHelpers.DisplaySuccessMessage("Model updated successfully.");
                    break;
                case "Change Purchase Date":
                    EditPurchaseDate(asset);
                    ConsoleHelpers.DisplaySuccessMessage("Purchase date updated successfully.");
                    break;
                case "Change Price":
                    EditPrice(asset);
                    ConsoleHelpers.DisplaySuccessMessage("Price updated successfully.");
                    break;
                case "Change Office":
                    EditOffice(asset);
                    ConsoleHelpers.DisplaySuccessMessage("Office updated successfully.");
                    break;
                case "Change Computer Type":
                    EditComputerType((Computer)asset);
                    ConsoleHelpers.DisplaySuccessMessage("Computer type updated successfully.");
                    break;
                case "Back to Main Menu":
                    OutputTracker.HasWritten = false; return;
            }
        }
    }

    internal static void HandleExportToCsv(AssetService assetService)
    {
        ConsoleHelpers.Heading("Export to CSV");

        List<string> sortOptions = ["Office", "Asset Type", "End of Life"];
        string sortChoice = ConsoleHelpers.SelectFromList(
            sortOptions,
            s => s,
            $"Sort exported assets by (1 - {sortOptions.Count} or \"q\" to quit): ",
            allowCancel: true);

        AssetSortMode sortMode = sortChoice switch
        {
            "Asset Type" => AssetSortMode.AssetType,
            "End of Life" => AssetSortMode.EndOfLife,
            _ => AssetSortMode.Office,
        };

        IReadOnlyList<Asset> assets = assetService.GetSortedAssets(sortMode);
        if (assets.Count == 0)
        {
            ConsoleHelpers.DisplayWarningMessage("No assets found.");
            return;
        }

        string fileName = ConsoleHelpers.ValidateInput<string>(
            "File name (without extension) (or \"q\" to quit): ",
            FormatHelpers.ValidateFileName(),
            "Invalid file name - avoid characters like \\ / : * ? \" < > |.",
            allowCancel: true);

        string filePath = Path.Combine(AppPaths.DataDirectory, $"{fileName}.csv");

        if (File.Exists(filePath) && !ConsoleHelpers.Confirm("A file with this name already exists. Are you sure you want to overwrite it?"))
        {
            ConsoleHelpers.DisplayWarningMessage("Cancelled - export was not saved.");
            return;
        }

        try
        {
            Directory.CreateDirectory(AppPaths.DataDirectory);
            int count = AssetService.ExportToCsv(assets, filePath, assetService.CurrencyProvider);
            ConsoleHelpers.DisplaySuccessMessage($"Exported {count} {FormatHelpers.Pluralize(count, "asset", "assets")} to {filePath}.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            ConsoleHelpers.DisplayErrorMessage($"Could not write file: {ex.Message}");
        }
    }

    internal static void HandleRemoveAsset(AssetService assetService)
    {
        ConsoleHelpers.Heading("Remove Asset");

        int? id = SelectAssetIdFromList(assetService, "remove");
        if (id is null)
        {
            return;
        }

        Asset asset;
        try
        {
            asset = assetService.GetById(id.Value);
        }
        catch (AssetNotFoundException ex)
        {
            ConsoleHelpers.DisplayErrorMessage(ex.Message);
            return;
        }

        bool confirmed = ConsoleHelpers.Confirm($"Remove {asset.Brand} {asset.Model} (Id {asset.Id})? This cannot be undone.");
        if (!confirmed)
        {
            ConsoleHelpers.DisplayWarningMessage("Cancelled - asset was not removed.");
            return;
        }

        assetService.RemoveAsset(asset.Id);
        ConsoleHelpers.DisplaySuccessMessage($"{asset.Brand} {asset.Model} (Id {asset.Id}) removed successfully.");
    }

    private static int? SelectAssetIdFromList(AssetService assetService, string actionLabel)
    {
        int currentPage = 1;

        while (true)
        {
            AssetsPage page = assetService.GetAssetsPage(AssetSortMode.Office, currentPage);
            currentPage = page.PageNumber;

            if (page.Items.Count == 0)
            {
                ConsoleHelpers.DisplayWarningMessage("No assets found.");
                return null;
            }

            ConsoleTableRenderer.RenderAssets(page.Items, assetService.CurrencyProvider);
            Console.WriteLine($"\nPage {page.PageNumber} of {page.TotalPages}\n");

            List<string> menuItems = ["Previous Page", "Next Page\n", $"Enter Asset Id to {actionLabel}", "Back to Main Menu"];
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
                case string s when s.StartsWith("Enter Asset Id"):
                    return ConsoleHelpers.ValidateInput(
                        "Asset Id (or \"q\" to quit): ",
                        ConsoleHelpers.ValidateIntegerRange(1, int.MaxValue),
                        "Invalid input, please enter a valid Id.",
                        allowCancel: true);
                case "Back to Main Menu":
                    OutputTracker.HasWritten = false; return null;
            }
        }
    }

    private static Asset ChangeAssetType(AssetService assetService, Asset asset)
    {
        List<string> assetTypes = ["Computer", "Mobile Phone", "Tablet"];
        string currentType = GetAssetTypeLabel(asset);

        string newType = ConsoleHelpers.SelectFromList(
            assetTypes,
            type => type,
            $"Select asset type (1 - {assetTypes.Count}, current: {currentType}): ");

        if (newType == currentType)
        {
            return asset;
        }

        ComputerType? computerType = newType == "Computer"
            ? ConsoleHelpers.SelectFromList(
                Enum.GetValues<ComputerType>().ToList(),
                t => t.ToString(),
                $"Select computer type (1 - {Enum.GetValues<ComputerType>().Length}): ")
            : null;

        Asset newAsset = newType switch
        {
            "Computer" => new Computer(asset.Id, asset.Brand, asset.Model, asset.PurchaseDate, asset.PriceEur, asset.Office, computerType!.Value),
            "Mobile Phone" => new MobilePhone(asset.Id, asset.Brand, asset.Model, asset.PurchaseDate, asset.PriceEur, asset.Office),
            "Tablet" => new Tablet(asset.Id, asset.Brand, asset.Model, asset.PurchaseDate, asset.PriceEur, asset.Office),
            _ => throw new InvalidOperationException("Unexpected asset type."),
        };

        assetService.ReplaceAsset(newAsset);
        return newAsset;
    }

    private static string GetAssetTypeLabel(Asset asset)
    {
        return asset switch
        {
            Computer => "Computer",
            MobilePhone => "Mobile Phone",
            Tablet => "Tablet",
            _ => throw new InvalidOperationException("Unexpected asset type."),
        };
    }

    private static void EditBrand(Asset asset)
    {
        asset.Brand = ConsoleHelpers.ValidateInput($"Brand (Just press enter to keep '{asset.Brand}'): ", currentValue: asset.Brand);
    }

    private static void EditModel(Asset asset)
    {
        asset.Model = ConsoleHelpers.ValidateInput($"Model (Just press enter to keep '{asset.Model}'): ", currentValue: asset.Model);
    }

    private static void EditPurchaseDate(Asset asset)
    {
        asset.PurchaseDate = ConsoleHelpers.ValidateInput(
            $"Purchase date (yyyy-MM-dd) (Just press enter to keep '{asset.PurchaseDate:yyyy-MM-dd}'): ",
            FormatHelpers.ValidateDate(),
            "Invalid date. Use format yyyy-MM-dd and don't pick a future date.",
            hasCurrentValue: true,
            currentValue: asset.PurchaseDate);
    }

    private static void EditPrice(Asset asset)
    {
        asset.PriceEur = ConsoleHelpers.ValidateInput(
            $"Price in EUR (Just press enter to keep '{FormatHelpers.FormatPrice(asset.PriceEur)}'): ",
            FormatHelpers.ValidatePositiveDecimal(),
            "Invalid price.",
            hasCurrentValue: true,
            currentValue: asset.PriceEur);
    }

    private static void EditOffice(Asset asset)
    {
        asset.Office = ConsoleHelpers.SelectFromList(
            Office.All,
            o => $"{o.Name} ({o.Currency})",
            $"Select office (1 - {Office.All.Count}, current: {asset.Office.Name}): ");
    }

    private static void EditComputerType(Computer computer)
    {
        List<ComputerType> computerTypes = Enum.GetValues<ComputerType>().ToList();
        computer.ComputerType = ConsoleHelpers.SelectFromList(
            computerTypes,
            t => t.ToString(),
            $"Select computer type (1 - {computerTypes.Count}, current: {computer.ComputerType}): ");
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
