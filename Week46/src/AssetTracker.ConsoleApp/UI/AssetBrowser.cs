using AssetTracker.Core.Models;
using AssetTracker.Core.Services;

namespace AssetTracker.ConsoleApp.UI;

/// <summary>
/// The paginated asset list. Holds the current <see cref="AssetQuery"/> and lets the user page
/// through and change sorting; the service does the sorting and paging in the database.
/// </summary>
internal sealed class AssetBrowser(AssetService assetService)
{
    /// <summary>
    /// Runs the list screen until "0. Back". Menu-driven only, so there's no typed prompt to cancel;
    /// a database failure is left to <see cref="ConsoleInput.TryRun(Action)"/>.
    /// </summary>
    internal void Show()
    {
        var query = new AssetQuery();

        while (true)
        {
            var page = assetService.GetPage(query);

            // Keep the page the repository actually returned, so Next/Previous start from it.
            query = query with { Page = page.PageNumber };

            ConsoleMessage.Heading("View assets");
            RenderPage(page, query);

            string[] menuItems =
            [
                "Previous page",
                "Next page\n",
                $"Change sorting (current: {ConsoleInput.EnumDisplayName(query.SortBy)})",
            ];

            var choice = ConsoleInput.SelectMenuOption(menuItems, "Back to main menu", PagingLimits(page));
            switch (choice)
            {
                case 0:
                    return;
                case 1:
                    query = query with { Page = query.Page - 1 };
                    break;
                case 2:
                    query = query with { Page = query.Page + 1 };
                    break;
                case 3:
                    query = ChangeSorting(query);
                    break;
            }
        }
    }

    private static void RenderPage(PagedResult<Asset> page, AssetQuery query)
    {
        if (page.TotalCount == 0)
        {
            ConsoleMessage.DisplayWarningMessage("No assets registered yet.");
        }
        else
        {
            AssetTable.Render(page.Items, DateOnly.FromDateTime(DateTime.Today));
        }

        var assetWord = page.TotalCount == 1 ? "asset" : "assets";
        Console.WriteLine(
            $"Page {page.PageNumber} of {page.TotalPages} · {page.TotalCount} {assetWord} · sorted by {ConsoleInput.EnumDisplayName(query.SortBy).ToLowerInvariant()}");
        Console.WriteLine();
    }

    /// <summary>Disables "Previous page" on the first page and "Next page" on the last.</summary>
    private static Dictionary<int, string> PagingLimits(PagedResult<Asset> page)
    {
        Dictionary<int, string> disabledChoices = [];
        if (page.PageNumber == 1)
        {
            disabledChoices[1] = "Already on the first page.";
        }

        if (page.PageNumber == page.TotalPages)
        {
            disabledChoices[2] = "Already on the last page.";
        }

        return disabledChoices;
    }

    /// <summary>Lets the user pick a sort order; a new order starts again from page 1.</summary>
    private static AssetQuery ChangeSorting(AssetQuery query)
    {
        Console.WriteLine();
        var sortBy = ConsoleInput.SelectEnumOption(query.SortBy);
        return sortBy == query.SortBy ? query : query with { SortBy = sortBy, Page = 1 };
    }
}
