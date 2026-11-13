using AssetTracker.Core.Models;
using AssetTracker.Core.Services;

namespace AssetTracker.ConsoleApp.UI;

/// <summary>The Reports menu: pick a report, see it, come back. The numbers come from <see cref="ReportService"/>.</summary>
internal sealed class ReportMenu(ReportService reportService)
{
    /// <summary>Runs until "0. Back". A database failure is left to <see cref="ConsoleInput.TryRun(Action)"/>.</summary>
    internal void Run()
    {
        (string Label, Func<Report> Build)[] reports =
        [
            ("Value and count per office", reportService.OfficeSummary),
        ];

        while (true)
        {
            ConsoleMessage.Heading("Reports");

            var choice = ConsoleInput.SelectMenuOption(reports.Select(r => r.Label).ToArray(), "Back to main menu");
            if (choice == 0)
            {
                return;
            }

            Show(reports[choice - 1].Build());
        }
    }

    private static void Show(Report report)
    {
        ConsoleMessage.Heading(report.Title);
        ReportTable.Render(report);
        ConsoleInput.Pause();
    }
}
