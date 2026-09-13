using System;
using System.Collections.Generic;
using System.Text;

namespace Week37
{
    internal static class DataMenu
    {
        internal static void HandleSaveData(string path, CategoryManager categoryManager, ProductManager productManager)
        {
            Utils.Heading("Save data");

            bool confirmed = Utils.Confirm("This will overwrite any previously saved data. Continue?");

            if (!confirmed)
            {
                Utils.DisplayWarningMessage("Save cancelled.");
                return;
            }

            DataStore.Save(path, categoryManager, productManager);
            Utils.DisplaySuccessMessage("Data saved successfully.");
        }

        internal static void HandleLoadData(string path, CategoryManager categoryManager, ProductManager productManager)
        {
            Utils.Heading("Load data");

            bool confirmed = Utils.Confirm("Loading will replace everything currently in memory with the contents of data file.\nAny unsaved changes will be lost. Continue?");

            if (!confirmed)
            {
                Utils.DisplayWarningMessage("Load cancelled.");
                return;
            }

            bool loaded = DataStore.Load(path, categoryManager, productManager);

            if (loaded)
            {
                Utils.DisplaySuccessMessage("Data successfully loaded.");
            }
            else
            {
                Utils.DisplayWarningMessage("No saved data found.");
            }
        }

        internal static void HandleResetData(ProductManager productManager, CategoryManager categoryManager)
        {
            Utils.Heading("Reset data");

            string[] menuItems = ["Reset products only", "Reset products and categories", "Cancel"];

            Utils.DisplayNumberedList(menuItems);

            int choice = Utils.ValidateInput(
                    $"Select option (1 - {menuItems.Length}): ",
                    Utils.ValidateIntegerRange(1, menuItems.Length),
                    $"Invalid input, please enter a number between 1 and {menuItems.Length}.");

            if (choice == 3)
            {
                return;
            }

            string target = choice == 1 ? "all products" : "all products and categories";
            string message =
                $"This will clear {target} from memory. This will NOT affect data.json - if you have saved " +
                "data, you can reload it afterward with \"Load Data\" from the main menu. Continue?";

            bool confirmed = Utils.Confirm(message);

            if (!confirmed)
            {
                Utils.DisplayWarningMessage("Reset cancelled.");
                return;
            }

            productManager.ClearAll();

            if (choice == 2)
            {
                categoryManager.ClearAll();
            }

            Utils.DisplaySuccessMessage("Reset completed successfully. Your saved file (if any) is untouched.");
        }

        internal static void HandleExit(string path, CategoryManager categoryManager, ProductManager productManager)
        {
            Utils.Heading("Exit");

            bool save = Utils.Confirm("Would you like to save before exiting?");

            if (save)
            {
                DataStore.Save(path, categoryManager, productManager);
                Utils.DisplaySuccessMessage("Data saved successfully.");
            }
            else
            {
                Utils.DisplayWarningMessage("Exiting without saving.");
            }
        }
    }
}
