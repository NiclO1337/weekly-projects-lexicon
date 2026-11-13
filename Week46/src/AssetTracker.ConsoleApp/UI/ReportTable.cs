using System.Globalization;
using AssetTracker.Core.Models;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace AssetTracker.ConsoleApp.UI;

/// <summary>
/// Draws any <see cref="Report"/> as a Spectre table, formatting each raw cell value for the screen.
/// Cells are <see cref="Text"/> widgets, not markup, so data containing "[" is shown as-is.
/// </summary>
internal static class ReportTable
{
    internal static void Render(Report report)
    {
        var headerStyle = new Style(Theme.Accent, decoration: Decoration.Bold);
        var table = new Table()
            .Border(TableBorder.Simple)
            .BorderStyle(new Style(Theme.Accent));

        foreach (var column in report.Columns)
        {
            var tableColumn = new TableColumn(new Text(column.Header, headerStyle));
            table.AddColumn(column.IsNumeric ? tableColumn.RightAligned() : tableColumn);
        }

        foreach (var row in report.Rows)
        {
            table.AddRow(row.Select(value => (IRenderable)new Text(FormatCell(value))).ToArray());
        }

        AnsiConsole.Write(table);
    }

    private static string FormatCell(object value) => value switch
    {
        decimal amount => Format.Amount(amount),
        DateOnly date => Format.Date(date),
        int number => number.ToString(CultureInfo.InvariantCulture),
        _ => value.ToString() ?? string.Empty,
    };
}
