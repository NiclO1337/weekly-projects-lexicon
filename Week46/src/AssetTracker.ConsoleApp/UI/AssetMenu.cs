using AssetTracker.Core.Enums;
using AssetTracker.Core.Models;
using AssetTracker.Core.Services;

namespace AssetTracker.ConsoleApp.UI;

/// <summary>
/// The asset forms: add, show details, edit. Collects input and shows results; the rules live in
/// <see cref="AssetService"/>. Add and Edit share the field prompts below: with a current value
/// Enter keeps it, without one the field is required.
/// </summary>
internal sealed class AssetMenu(AssetService assetService)
{
    private const string CancelHint = "Type q at a prompt, or choose 0 in a menu, to cancel.";

    /// <summary>
    /// Asks for every field of a new asset and saves it. "q" at a typed prompt or "0" in a menu
    /// cancels (<see cref="UserCancelledException"/>, handled by <see cref="ConsoleInput.TryRun(Func{bool})"/>).
    /// </summary>
    internal bool AddAsset()
    {
        ConsoleMessage.Heading("Add asset");
        ConsoleMessage.WriteColoredLine(CancelHint, Theme.Accent);

        var (addAsset, standardWarrantyYears) = SelectAssetKind();
        var brand = ReadText("Brand", Asset.MaxBrandLength);
        var model = ReadText("Model", Asset.MaxModelLength);
        var serialNumber = ReadSerialNumber();

        var today = Today();
        var purchaseDate = ReadPurchaseDate(today, $"Enter for today {Format.Date(today)}");
        var standardWarranty = purchaseDate.AddYears(standardWarrantyYears);
        var warrantyExpirationDate = ReadWarrantyDate(
            purchaseDate,
            standardWarranty,
            $"Enter for {Format.Date(standardWarranty)}, the standard {standardWarrantyYears} years");

        var office = SelectOffice("Office:");
        var price = ReadPrice(office.Currency);

        // An empty current value lets Enter skip the prompt; the domain stores blank as "unassigned".
        var assignedTo = ConsoleInput.ValidateInput("Assigned to (Enter to skip): ", Asset.MaxAssignedToLength, currentValue: string.Empty);

        var asset = addAsset(new AssetDetails(brand, model, serialNumber, purchaseDate, warrantyExpirationDate, price, office.Id, assignedTo));

        ConsoleMessage.NewLine();
        ConsoleMessage.DisplaySuccessMessage(
            $"Added {asset.TypeLabel.ToLowerInvariant()} {asset.Brand} {asset.Model} (ID {asset.Id}) to the {asset.Office.Name} office.");
        return true;
    }

    /// <summary>Shows every field of <paramref name="asset"/>. Returns <see langword="true"/> so the browser pauses on it.</summary>
    internal bool ShowDetails(Asset asset)
    {
        ConsoleMessage.Heading("Asset details");
        AssetDetailsPanel.Render(asset, Today());
        return true;
    }

    /// <summary>
    /// Edits one field or all of them, then saves once. Nothing is saved until every prompt is
    /// answered, so "q" anywhere leaves the asset unchanged. Returns <see langword="false"/> if
    /// the user picked "0. Cancel" straight away.
    /// </summary>
    internal bool EditAsset(Asset asset)
    {
        ConsoleMessage.Heading("Edit asset");
        AssetDetailsPanel.Render(asset, Today());
        ConsoleMessage.WriteColoredLine("Press Enter at a prompt to keep the current value. " + CancelHint, Theme.Accent);

        (string Label, Func<AssetDetails, AssetDetails> Edit)[] fields =
        [
            ("Brand", d => d with { Brand = ReadText("Brand", Asset.MaxBrandLength, d.Brand) }),
            ("Model", d => d with { Model = ReadText("Model", Asset.MaxModelLength, d.Model) }),
            ("Serial number", d => d with { SerialNumber = ReadSerialNumber(d.SerialNumber, asset.Id) }),
            ("Purchase date", EditPurchaseDate),
            ("Warranty expiration", d => d with { WarrantyExpirationDate = ReadWarrantyDate(d.PurchaseDate, d.WarrantyExpirationDate, Keep(Format.Date(d.WarrantyExpirationDate))) }),
            ("Price", d => d with { PurchasePrice = ReadPrice(asset.Currency, d.PurchasePrice) }),
            ("Office", d => d with { OfficeId = SelectOffice($"Office (the price stays in {asset.Currency}):", d.OfficeId).Id }),
            ("Assigned to", d => d with { AssignedTo = ReadAssignedTo(d.AssignedTo) }),
        ];

        ConsoleMessage.NewLine();
        ConsoleMessage.WriteColoredLine("Edit which field?", Theme.Accent);
        var choice = ConsoleInput.SelectMenuOption(["All fields", .. fields.Select(f => f.Label)], "Cancel");
        if (choice == 0)
        {
            return false;
        }

        var current = ToDetails(asset);

        // "All fields" runs every prompt in order, each seeing the answers before it.
        var updated = choice == 1
            ? fields.Aggregate(current, (details, field) => field.Edit(details))
            : fields[choice - 2].Edit(current);

        var saved = assetService.Update(asset.Id, updated);
        ConsoleMessage.NewLine();
        ConsoleMessage.DisplaySuccessMessage($"Saved {saved.TypeLabel.ToLowerInvariant()} {saved.Brand} {saved.Model} (ID {saved.Id}).");
        return true;
    }

    /// <summary>
    /// Asks for a new purchase date. If the current warranty would now end before it, asks for a
    /// new warranty date too, since keeping it isn't allowed.
    /// </summary>
    private static AssetDetails EditPurchaseDate(AssetDetails details)
    {
        var updated = details with { PurchaseDate = ReadPurchaseDate(details.PurchaseDate, Keep(Format.Date(details.PurchaseDate))) };
        if (updated.WarrantyExpirationDate >= updated.PurchaseDate)
        {
            return updated;
        }

        ConsoleMessage.DisplayWarningMessage("The warranty would now expire before the purchase date. Enter a new warranty date.");
        return updated with { WarrantyExpirationDate = ReadWarrantyDate(updated.PurchaseDate) };
    }

    private static AssetDetails ToDetails(Asset asset) => new(
        asset.Brand,
        asset.Model,
        asset.SerialNumber,
        asset.PurchaseDate,
        asset.WarrantyExpirationDate,
        asset.PurchasePrice,
        asset.OfficeId,
        asset.AssignedTo);

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

    private static string ReadText(string label, int maxLength, string? current = null) =>
        ConsoleInput.ValidateInput(Prompt(label, current is null ? null : Keep(current)), maxLength, current);

    /// <summary>Re-prompts until the serial number isn't registered to another asset.</summary>
    private string ReadSerialNumber(string? current = null, int? exceptAssetId = null)
    {
        while (true)
        {
            var serialNumber = ReadText("Serial number", Asset.MaxSerialNumberLength, current).Trim();
            if (!assetService.SerialNumberExists(serialNumber, exceptAssetId))
            {
                return serialNumber;
            }

            ConsoleMessage.DisplayErrorMessage($"Serial number {serialNumber} is already registered.");
        }
    }

    private static DateOnly ReadPurchaseDate(DateOnly defaultValue, string defaultHint) =>
        ReadDate("Purchase date", DateOnly.MinValue, Today(), "not in the future", defaultValue, defaultHint);

    private static DateOnly ReadWarrantyDate(DateOnly purchaseDate, DateOnly? defaultValue = null, string? defaultHint = null) =>
        ReadDate("Warranty expiration", purchaseDate, DateOnly.MaxValue, $"on or after {Format.Date(purchaseDate)}", defaultValue, defaultHint);

    /// <summary>A date between <paramref name="earliest"/> and <paramref name="latest"/>; Enter gives <paramref name="defaultValue"/> when there is one.</summary>
    private static DateOnly ReadDate(string label, DateOnly earliest, DateOnly latest, string rule, DateOnly? defaultValue, string? defaultHint) =>
        ConsoleInput.ValidateInput(
            Prompt($"{label} ({Format.DateFormat})", defaultHint),
            InputValidators.Date(earliest, latest),
            $"Enter a date as {Format.DateFormat}, {rule}.",
            hasCurrentValue: defaultValue is not null,
            currentValue: defaultValue ?? default);

    private static decimal ReadPrice(CurrencyCode currency, decimal? current = null) =>
        ConsoleInput.ValidateInput(
            Prompt($"Price in {currency}", current is null ? null : Keep(Format.Amount(current.Value))),
            InputValidators.Price(),
            $"Enter a price above 0 and at most {Format.Amount(Asset.MaxPurchasePrice)}, e.g. 14990, 14 990, 1499,50 or 15.000,37.",
            hasCurrentValue: current is not null,
            currentValue: current ?? default);

    /// <summary>Enter keeps the current user; "-" clears it.</summary>
    private static string? ReadAssignedTo(string? current)
    {
        var hint = current is null ? "Enter to keep nobody" : $"{Keep(current)}, - for nobody";
        var input = ConsoleInput.ValidateInput(Prompt("Assigned to", hint), Asset.MaxAssignedToLength, current ?? string.Empty);
        return input.Trim() == "-" ? null : input;
    }

    private Office SelectOffice(string title, int? currentOfficeId = null) =>
        SelectOption(
            title,
            assetService.GetOffices(),
            o => $"{o.Name} ({o.Currency}){(o.Id == currentOfficeId ? " (current)" : "")}");

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

    /// <summary>"Label: " or "Label (hint): ".</summary>
    private static string Prompt(string label, string? hint) => hint is null ? $"{label}: " : $"{label} ({hint}): ";

    private static string Keep(string value) => $"Enter to keep {value}";

    private static DateOnly Today() => DateOnly.FromDateTime(DateTime.Today);
}
