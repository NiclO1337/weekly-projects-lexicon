namespace AssetTracker.ConsoleApp.UI;

/// <summary>
/// The console UI's root shell: the top-level menu loop and its "Quit" condition. Owns
/// navigation only; each action is delegated to the screen that handles it.
/// </summary>
internal sealed class MainMenu(AssetBrowser assetBrowser, AssetMenu assetMenu)
{
    internal void Run()
    {
        try
        {
            RunMenuLoop();
        }
        catch (UserCancelledException)
        {
            // Input stream closed (e.g. piped input reached its end) while reading the top-level
            // menu choice, which isn't wrapped by ConsoleInput.TryRun. Exit gracefully, same as
            // choosing "0. Quit".
        }
    }

    private void RunMenuLoop()
    {
        string[] menuItems = ["View assets", "Add asset", "Edit asset"];

        while (true)
        {
            ConsoleMessage.MainHeading("DRAGON'S HOARD - ASSET TRACKING SYSTEM");

            var choice = ConsoleInput.SelectMenuOption(menuItems, "Quit");

            switch (choice)
            {
                case 0:
                    return;
                case 1:
                    ConsoleInput.TryRun(() => assetBrowser.Browse("View assets", "Show asset details", assetMenu.ShowDetails));
                    break;
                case 2:
                    ConsoleInput.TryRun(assetMenu.AddAsset);
                    break;
                case 3:
                    ConsoleInput.TryRun(() => assetBrowser.Browse("Edit asset", "Edit an asset", assetMenu.EditAsset));
                    break;
            }
        }
    }
}
