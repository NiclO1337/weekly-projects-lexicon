namespace AssetTracker.UI;

internal static class MainMenu
{
    private static int DisplayMainMenu(bool pauseFirst)
    {
        if (pauseFirst)
        {
            Console.Write("\nPress any key to continue to main menu...");
            Console.ReadKey();
        }

        Console.WriteLine("\n================================================");
        Console.WriteLine("   COMPANY ASSET TRACKING SYSTEM");
        Console.WriteLine("================================================\n");

        string[] menuItems =
        [
            "Add Asset",
            "View Assets",
            "Search Assets",
            "Edit Assets",
            "Remove Asset",
            "Export to CSV",
            "Exit",
        ];

        ConsoleHelpers.DisplayNumberedList(menuItems);

        return menuItems.Length;
    }

    internal static void RunMainMenu()
    {
        OutputTracker.HasWritten = true; // pause once, right after the intro text

        while (true)
        {
            bool hasNewOutput = OutputTracker.HasWritten;
            int optionCount = DisplayMainMenu(hasNewOutput);

            int choice = ConsoleHelpers.ValidateInput($"Select option (1 - {optionCount}): ",
                ConsoleHelpers.ValidateIntegerRange(1, optionCount));

            // clear the menu+prompt noise; only the handler's output counts now
            OutputTracker.HasWritten = false;

            switch (choice)
            {
                case 1:
                    // AssetMenu.HandleAddAsset();
                    ConsoleHelpers.DisplayWarningMessage("Not implemented yet.");
                    break;
                case 2:
                    // AssetMenu.HandleViewAssets();
                    ConsoleHelpers.DisplayWarningMessage("Not implemented yet.");
                    break;
                case 3:
                    // AssetMenu.HandleSearchAssets();
                    ConsoleHelpers.DisplayWarningMessage("Not implemented yet.");
                    break;
                case 4:
                    // AssetMenu.HandleEditAssets();
                    ConsoleHelpers.DisplayWarningMessage("Not implemented yet.");
                    break;
                case 5:
                    // AssetMenu.HandleRemoveAsset();
                    ConsoleHelpers.DisplayWarningMessage("Not implemented yet.");
                    break;
                case 6:
                    // AssetMenu.HandleExportToCsv();
                    ConsoleHelpers.DisplayWarningMessage("Not implemented yet.");
                    break;
                case 7:
                    return;
            }
        }
    }
}
