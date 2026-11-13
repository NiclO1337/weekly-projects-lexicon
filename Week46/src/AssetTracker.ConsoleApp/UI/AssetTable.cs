using System.Globalization;
using AssetTracker.Core.Models;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace AssetTracker.ConsoleApp.UI;

/// <summary>
/// Draws a list of assets as a Spectre table. Every cell is a <see cref="Text"/> widget, not
/// markup, so brand or model text containing "[" is shown as-is.
/// </summary>
internal static class AssetTable
{
    /// <param name="assets">The rows to draw.</param>
    /// <param name="today">The date lifetime status is measured against.</param>
    /// <param name="valueInEur">Each asset's value in EUR, from the service; the UI does no currency math.</param>
    internal static void Render(IReadOnlyList<Asset> assets, DateOnly today, Func<Asset, decimal> valueInEur)
    {
        var headerStyle = new Style(Theme.Accent, decoration: Decoration.Bold);
        var table = new Table()
            .Border(TableBorder.Simple)
            .BorderStyle(new Style(Theme.Accent))
            .AddColumns(
                Column("Id", headerStyle).RightAligned(),
                Column("Type", headerStyle),
                Column("Brand", headerStyle),
                Column("Model", headerStyle),
                Column("Purchased", headerStyle),
                Column("Office", headerStyle),
                Column("Price", headerStyle).RightAligned(),
                Column("Code", headerStyle),
                Column("Value (EUR)", headerStyle).RightAligned(),
                Column("EOL status", headerStyle));

        foreach (var asset in assets)
        {
            table.AddRow(Cells(asset, today, valueInEur(asset)));
        }

        AnsiConsole.Write(table);
    }

    private static TableColumn Column(string header, Style style) => new(new Text(header, style));

    private static IRenderable[] Cells(Asset asset, DateOnly today, decimal valueInEur)
    {
        // Lifetime status colours the purchase date and the label only.
        var lifetimeStyle = Theme.EndOfLife(asset.GetEndOfLifeStatus(today));
        return
        [
            new Text(asset.Id.ToString(CultureInfo.InvariantCulture)),
            new Text(asset.TypeLabel),
            new Text(asset.Brand),
            new Text(asset.Model),
            new Text(Format.Date(asset.PurchaseDate), lifetimeStyle),
            new Text(asset.Office.Name),
            new Text(Format.Amount(asset.PurchasePrice)),
            new Text(asset.Currency.ToString()),
            new Text(Format.Amount(valueInEur)),
            new Text(asset.GetEndOfLifeStatusLabel(today), lifetimeStyle),
        ];
    }
}
