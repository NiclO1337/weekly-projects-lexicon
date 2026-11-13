using AssetTracker.Core.Exceptions;
using AssetTracker.Core.Models;
using AssetTracker.Core.Services;

namespace AssetTracker.ConsoleApp.UI;

/// <summary>
/// The paginated asset list, shared by View, Edit and Remove: each passes its own heading and the
/// action to run on an asset picked by ID. Holds the current <see cref="AssetQuery"/>; the service
/// does the sorting and paging in the database.
/// </summary>
internal sealed class AssetBrowser(AssetService assetService)
{
    /// <summary>
    /// Runs the list screen until "0. Back". A database failure is left to
    /// <see cref="ConsoleInput.TryRun(Action)"/>.
    /// </summary>
    /// <param name="heading">The screen heading, e.g. "Edit asset".</param>
    /// <param name="idActionLabel">The menu entry for the ID action, e.g. "Edit an asset".</param>
    /// <param name="action">
    /// Runs on the picked asset. Returns <see langword="false"/> if it backed out without doing
    /// anything, so there's nothing to pause on before the list is redrawn.
    /// </param>
    internal void Browse(string heading, string idActionLabel, Func<Asset, bool> action)
    {
        var query = new AssetQuery();

        while (true)
        {
            var page = assetService.GetPage(query);

            // Keep the page the repository actually returned, so Next/Previous start from it.
            query = query with { Page = page.PageNumber };

            ConsoleMessage.Heading(heading);
            RenderPage(page, query);

            string[] menuItems =
            [
                "Previous page",
                "Next page\n",
                $"Change sorting (current: {ConsoleInput.EnumDisplayName(query.SortBy)})",
                idActionLabel,
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
                case 4:
                    PromptIdAndRunAction(action);
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

    /// <summary>
    /// Asks for an asset ID and runs <paramref name="action"/> on that asset. A "q" cancel or an
    /// unknown ID is handled here rather than in <see cref="ConsoleInput.TryRun(Action)"/>, so the
    /// user stays on this list instead of being sent back to the main menu.
    /// </summary>
    private void PromptIdAndRunAction(Func<Asset, bool> action)
    {
        var shouldPause = true;
        try
        {
            var id = ConsoleInput.ValidateInput(
                "Asset ID (q to cancel): ",
                ConsoleInput.ValidateIntegerRange(1, int.MaxValue),
                "Enter an asset ID from the list.");
            shouldPause = action(assetService.GetById(id));
        }
        catch (AssetNotFoundException ex)
        {
            ConsoleMessage.DisplayErrorMessage(ex.Message);
        }
        catch (UserCancelledException)
        {
            ConsoleMessage.DisplayWarningMessage("Cancelled - nothing was changed.");
        }

        if (shouldPause)
        {
            ConsoleInput.Pause();
        }
    }
}
