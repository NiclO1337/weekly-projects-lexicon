using System;
using System.Collections.Generic;
using System.Text;

namespace Week37
{
    internal class MainMenu
    {
        private static int DisplayMainMenu(bool pauseFirst)
        {
            if (pauseFirst)
            {
                Console.Write("\nPress any key to continue to main menu...");
                Console.ReadKey();
            }

            Console.WriteLine("\n\n================================================");
            Console.WriteLine("   DRAGON'S HOARD - PRODUCT MANAGEMENT SYSTEM");
            Console.WriteLine("================================================\n");

            string[] menuItems =
            [
                " Add Product",
                " Show Products",
                " Search Product",
                " Edit Product",
                " Delete Product",
                " Statistics",
                " Save Data",
                " Load Data",
                " Reset Data",
                "Manage Categories",
                "Exit",
            ];

            Utils.DisplayNumberedList(menuItems);

            return menuItems.Length;
        }

        internal static void RunMainMenu(ProductManager productManager, CategoryManager categoryManager)
        {
            string dataFilePath = "data.json";
            OutputTracker.HasWritten = true; // pause once, right after the intro text

            while (true)
            {
                bool hasNewOutput = OutputTracker.HasWritten;
                int optionCount = DisplayMainMenu(hasNewOutput);

                int choice = Utils.ValidateInput($"Select option (1 - {optionCount}): ",
                    Utils.ValidateIntegerRange(1, optionCount));

                // clear the menu+prompt noise; only the handler´s output counts now
                OutputTracker.HasWritten = false;

                switch (choice)
                {
                    case 1:
                        Utils.TryRun(
                        () => ProductMenu.HandleAddProduct(productManager, categoryManager)
                        ); break;
                    case 2: productManager.ShowProducts(); break;
                    case 3: ProductMenu.HandleSearchProduct(productManager, categoryManager); break;
                    case 4:
                        Utils.TryRun(
                    () => ProductMenu.HandleEditProduct(productManager, categoryManager)
                    ); break;
                    case 5:
                        Utils.TryRun(
                    () => ProductMenu.HandleDeleteProduct(productManager)
                    ); break;
                    case 6: productManager.ShowStatictics(); break;
                    case 7: DataMenu.HandleSaveData(dataFilePath, categoryManager, productManager); break;
                    case 8: DataMenu.HandleLoadData(dataFilePath, categoryManager, productManager); break;
                    case 9: DataMenu.HandleResetData(productManager, categoryManager); break;
                    case 10: CategoryMenu.RunCategoryMenu(categoryManager, productManager); break;
                    case 11: DataMenu.HandleExit(dataFilePath, categoryManager, productManager); return;
                }
            }
        }
    }
}
