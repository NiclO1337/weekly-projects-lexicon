using System.Globalization;
using AssetTracker.Core.Models;
using Spectre.Console;

namespace AssetTracker.ConsoleApp.UI;

/// <summary>
/// Draws one asset with every field, as a two-column grid in a panel. Values are <see cref="Text"/>
/// widgets, not markup, so data containing "[" is shown as-is.
/// </summary>
internal static class AssetDetailsPanel
{
    internal static void Render(Asset asset, DateOnly today)
    {
        var labelStyle = new Style(Theme.Accent);
        var lifetimeStyle = Theme.EndOfLife(asset.GetEndOfLifeStatus(today));
        var statusLabel = asset.GetEndOfLifeStatusLabel(today);

        var grid = new Grid().AddColumn().AddColumn();
        (string Label, string Value, Style Style)[] rows =
        [
            ("Id", asset.Id.ToString(CultureInfo.InvariantCulture), Style.Plain),
            ("Type", $"{ConsoleInput.EnumDisplayName(asset.Category)} - {asset.TypeLabel}", Style.Plain),
            ("Brand", asset.Brand, Style.Plain),
            ("Model", asset.Model, Style.Plain),
            ("Serial number", asset.SerialNumber, Style.Plain),
            ("Purchase date", Format.Date(asset.PurchaseDate), lifetimeStyle),
            ("Warranty expires", Format.Date(asset.WarrantyExpirationDate), Style.Plain),
            ("Price", $"{Format.Amount(asset.PurchasePrice)} {asset.Currency}", Style.Plain),
            ("Office", asset.Office.Name, Style.Plain),
            ("Assigned to", asset.AssignedTo ?? "(nobody)", Style.Plain),
            ("Lifetime", statusLabel.Length == 0 ? "Within lifetime" : statusLabel, lifetimeStyle),
        ];

        foreach (var (label, value, style) in rows)
        {
            grid.AddRow(new Text(label, labelStyle), new Text(value, style));
        }

        var panel = new Panel(grid)
            .Header($" {Markup.Escape($"{asset.Brand} {asset.Model}")} ")
            .BorderStyle(labelStyle)
            .Padding(2, 1);

        AnsiConsole.Write(panel);
    }
}
