using AssetTracker.Core.Enums;
using AssetTracker.Core.Models;
using AssetTracker.Core.Services;

namespace AssetTracker.ConsoleApp.UI;

/// <summary>The asset screens: listing and adding. Collects input and shows results; the rules live in <see cref="AssetService"/>.</summary>
internal sealed class AssetMenu(AssetService assetService)
{
    /// <summary>Shows every asset. Returns <see langword="true"/> so <see cref="ConsoleInput.TryRun(Func{bool})"/> pauses on the list.</summary>
    internal bool ViewAssets()
    {
        ConsoleMessage.Heading("View assets");

        var assets = assetService.GetAll();
        if (assets.Count == 0)
        {
            ConsoleMessage.DisplayWarningMessage("No assets registered yet.");
            return true;
        }

        AssetTable.Render(assets, Today());
        return true;
    }

    /// <summary>
    /// Asks for every field of a new asset and saves it. "q" at a typed prompt or "0" in a menu
    /// cancels (<see cref="UserCancelledException"/>, handled by <see cref="ConsoleInput.TryRun(Func{bool})"/>).
    /// </summary>
    internal bool AddAsset()
    {
        ConsoleMessage.Heading("Add asset");
        ConsoleMessage.WriteColoredLine("Type q at a prompt, or choose 0 in a menu, to cancel.", Theme.Accent);

        var (addAsset, standardWarrantyYears) = SelectAssetKind();
        var brand = ConsoleInput.ValidateInput("Brand: ", Asset.MaxBrandLength);
        var model = ConsoleInput.ValidateInput("Model: ", Asset.MaxModelLength);
        var serialNumber = ReadNewSerialNumber();

        // Enter accepts the suggested date shown in the prompt.
        var today = Today();
        var purchaseDate = ConsoleInput.ValidateInput(
            $"Purchase date ({Format.DateFormat}, Enter for today {Format.Date(today)}): ",
            InputValidators.Date(DateOnly.MinValue, today),
            $"Enter a date as {Format.DateFormat}, not in the future.",
            hasCurrentValue: true,
            currentValue: today);

        var standardWarranty = purchaseDate.AddYears(standardWarrantyYears);
        var warrantyExpirationDate = ConsoleInput.ValidateInput(
            $"Warranty expiration ({Format.DateFormat}, Enter for {Format.Date(standardWarranty)}, the standard {standardWarrantyYears} years): ",
            InputValidators.Date(purchaseDate, DateOnly.MaxValue),
            $"Enter a date as {Format.DateFormat}, on or after {Format.Date(purchaseDate)}.",
            hasCurrentValue: true,
            currentValue: standardWarranty);

        var office = SelectOption("Office:", assetService.GetOffices(), o => $"{o.Name} ({o.Currency})");
        var price = ConsoleInput.ValidateInput(
            $"Price in {office.Currency}: ",
            InputValidators.Price(),
            $"Enter a price above 0 and at most {Format.Amount(Asset.MaxPurchasePrice)}, e.g. 14990, 14 990, 1499,50 or 15.000,37.");

        // An empty current value lets Enter skip the prompt; the domain stores blank as "unassigned".
        var assignedTo = ConsoleInput.ValidateInput("Assigned to (Enter to skip): ", Asset.MaxAssignedToLength, currentValue: string.Empty);

        var asset = addAsset(new AssetDetails(brand, model, serialNumber, purchaseDate, warrantyExpirationDate, price, office.Id, assignedTo));

        ConsoleMessage.NewLine();
        ConsoleMessage.DisplaySuccessMessage(
            $"Added {asset.TypeLabel.ToLowerInvariant()} {asset.Brand} {asset.Model} (ID {asset.Id}) to the {asset.Office.Name} office.");
        return true;
    }

    /// <summary>Asks for category, then type, and returns the matching service call and that kind's standard warranty.</summary>
    private (Func<AssetDetails, Asset> Add, int StandardWarrantyYears) SelectAssetKind()
    {
        switch (SelectEnum<AssetCategory>("Category:"))
        {
            case AssetCategory.Computer:
                var computerType = SelectEnum<ComputerType>("Computer type:");
                return (details => assetService.AddComputer(computerType, details), ComputerAsset.StandardWarrantyYears);
            case AssetCategory.Mobile:
                var mobileType = SelectEnum<MobileType>("Mobile type:");
                return (details => assetService.AddMobile(mobileType, details), MobileAsset.StandardWarrantyYears);
            default:
                throw new InvalidOperationException("Unknown asset category.");
        }
    }

    /// <summary>Re-prompts until the serial number isn't already registered.</summary>
    private string ReadNewSerialNumber()
    {
        while (true)
        {
            var serialNumber = ConsoleInput.ValidateInput("Serial number: ", Asset.MaxSerialNumberLength).Trim();
            if (!assetService.SerialNumberExists(serialNumber))
            {
                return serialNumber;
            }

            ConsoleMessage.DisplayErrorMessage($"Serial number {serialNumber} is already registered.");
        }
    }

    private static T SelectEnum<T>(string title)
        where T : struct, Enum =>
        SelectOption(title, Enum.GetValues<T>(), value => ConsoleInput.EnumDisplayName(value));

    /// <summary>A numbered menu of <paramref name="options"/>; "0. Cancel" throws <see cref="UserCancelledException"/>.</summary>
    private static T SelectOption<T>(string title, IReadOnlyList<T> options, Func<T, string> label)
    {
        ConsoleMessage.NewLine();
        ConsoleMessage.WriteColoredLine(title, Theme.Accent);

        var choice = ConsoleInput.SelectMenuOption(options.Select(label).ToArray(), "Cancel");
        return choice == 0 ? throw new UserCancelledException() : options[choice - 1];
    }

    private static DateOnly Today() => DateOnly.FromDateTime(DateTime.Today);
}
