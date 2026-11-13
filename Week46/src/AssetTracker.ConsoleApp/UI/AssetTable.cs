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
    internal static void Render(IReadOnlyList<Asset> assets, DateOnly today)
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
                Column("Serial number", headerStyle),
                Column("Purchased", headerStyle),
                Column("Office", headerStyle),
                Column("Price", headerStyle).RightAligned(),
                Column("Code", headerStyle),
                Column("EOL status", headerStyle));

        foreach (var asset in assets)
        {
            table.AddRow(Cells(asset, today));
        }

        AnsiConsole.Write(table);
    }

    private static TableColumn Column(string header, Style style) => new(new Text(header, style));

    private static IRenderable[] Cells(Asset asset, DateOnly today)
    {
        // Lifetime status colours the purchase date and the label only.
        var lifetimeStyle = Theme.EndOfLife(asset.GetEndOfLifeStatus(today));
        return
        [
            new Text(asset.Id.ToString(CultureInfo.InvariantCulture)),
            new Text(asset.TypeLabel),
            new Text(asset.Brand),
            new Text(asset.Model),
            new Text(asset.SerialNumber),
            new Text(Format.Date(asset.PurchaseDate), lifetimeStyle),
            new Text(asset.Office.Name),
            new Text(Format.Amount(asset.PurchasePrice)),
            new Text(asset.Currency.ToString()),
            new Text(asset.GetEndOfLifeStatusLabel(today), lifetimeStyle),
        ];
    }
}
