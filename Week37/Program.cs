namespace Week37
{
    internal class Program
    {
        static void Main()
        {
            CategoryManager categoryManager = new();
            ProductManager productManager = new(categoryManager);

            Console.WriteLine("\r\n                                               _   __,----'~~~~~~~~~`-----.__\r\n                                        .  .    `//====-              ____,-'~`\r\n                        -.            \\_|// .   /||\\\\  `~~~~`---.___./\r\n                  ______-==.       _-~o  `\\/    |||  \\\\           _,'`\r\n            __,--'   ,=='||\\=_    ;_,_,/ _-'|-   |`\\   \\\\        ,'\r\n         _-'      ,='    | \\\\`.    '',/~7  /-   /  ||   `\\.     /\r\n       .'       ,'       |  \\\\  \\_  \"  /  /-   /   ||      \\   /\r\n      / _____  /         |     \\\\.`-_/  /|- _/   ,||       \\ /\r\n     ,-'     `-|--'~~`--_ \\     `==-/  `| \\'--===-'       _/`\r\n               '         `-|      /|    )-'\\~'      _,--\"'\r\n                           '-~^\\_/ |    |   `\\_   ,^             /\\\r\n                                /  \\     \\__   \\/~               `\\__\r\n                            _,-' _/'\\ ,-'~____-'`-/                 ``===\\\r\n                           ((->/'    \\|||' `.     `\\.  ,                _||\r\n             ./                       \\_     `\\      `~---|__i__i__\\--~'_/\r\n            <_n_                     __-^-_    `)  \\-.______________,-~'\r\n             `B'\\)                  ///,-'~`__--^-  |-------~~~~^'\r\n             /^>                           ///,--~`-\\\r\n            `  `                                       -Tua Xiong");

            Console.WriteLine("\nWelcome to Dragon's hoard - guard your products well.\n");
            Console.WriteLine("A treasure-keeper's ledger for tracking your wares:");
            Console.WriteLine("add new stock to the hoard, search the vault, edit or");
            Console.WriteLine("retire old items, and check your riches at a glance.\n");

            RunMainMenu(productManager, categoryManager);

            Utils.Heading("Closing application...");

            Console.WriteLine("\nThe hoard is secure and the ledger is closed... for now.\n" +
                "Farewell, treasure keeper!\n\n" +
                "                        \\`-\\`-._\r\n                         \\` )`. `-.__      ,\r\n      '' , . _       _,-._;'_,-`__,-'    ,/\r\n     : `. ` , _' :- '--'._ ' `------._,-;'\r\n      `- ,`- '            `--..__,,---'   hh");



        }
        static int DisplayMainMenu()
        {
            Console.Write("\nPress any key to continue to main menu...");
            Console.ReadKey();

            Console.WriteLine("\n\n================================================");
            Console.WriteLine("   DRAGON'S HOARD - PRODUCT MANAGEMENT SYSTEM");
            Console.WriteLine("================================================\n");

            string[] menuItems =
            [
                "Add Product",
                "Show Products",
                "Search Product",
                "Edit Product",
                "Delete Product",
                "Statistics",
                "Save Data",
                "Load Data",
                "Reset Data",
                "Manage Categories",
                "Exit",
            ];

            for (int i = 0; i < menuItems.Length; i++)
            {
                Console.WriteLine($"{i + 1}. {menuItems[i]}");
            }
            return menuItems.Length;
        }

        static void RunMainMenu(ProductManager productManager, CategoryManager categoryManager)
        {
            string dataFilePath = "data.json";

            while (true)
            {
                int optionCount = DisplayMainMenu();
                int choice = Utils.ValidateInput($"Select option (1 - {optionCount}): ",
                    Utils.ValidateIntegerRange(1, optionCount));

                switch (choice)
                {
                    case 1: HandleAddProduct(productManager, categoryManager); break;
                    case 2: productManager.ShowProducts(); break;
                    case 3: HandleSearchProduct(productManager, categoryManager); break;
                    case 4: HandleEditProduct(productManager, categoryManager); break;
                    case 5: HandleDeleteProduct(productManager); break;
                    case 6: productManager.ShowStatictics(); break;
                    case 7: HandleSaveData(dataFilePath, categoryManager, productManager); break;
                    case 8: HandleLoadData(dataFilePath, categoryManager, productManager); break;
                    case 9: HandleResetData(productManager, categoryManager); break;
                    case 10: RunCategoryMenu(categoryManager, productManager); break;
                    case 11: HandleExit(dataFilePath, categoryManager, productManager); return;
                }
            }
        }
        static Category SelectOrCreateCategory(CategoryManager categoryManager)
        {
            while (true)
            {
                var categories = categoryManager.GetAll();

                if (categories.Count == 0)
                {
                    Utils.DisplayWarningMessage("No categories exist yet. Let's add one.");
                    return AddCategoryLoop(categoryManager);
                }

                Console.WriteLine("\n0. Add new category");
                for (int i = 0; i < categories.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {categories[i].Name}");
                }

                int choice = Utils.ValidateInput(
                    $"Select category (1 - {categories.Count}): ",
                    Utils.ValidateIntegerRange(0, categories.Count),
                    $"Invalid input, please enter a number between 1 and {categories.Count}.");

                if (choice == 0)
                {
                    return AddCategoryLoop(categoryManager);
                }

                return categories[choice - 1];
            }
        }

        static Category? HandleAddCategory(CategoryManager categoryManager)
        {
            Utils.Heading("Add Category");
            string name = Utils.ValidateInput("Enter category name: ");
            Category? category = categoryManager.AddCategory(name);

            if (category is null)
            {
                Utils.DisplayErrorMessage("A category with that name already exists.");
                return null;
            }

            Utils.DisplaySuccessMessage($"Category '{category.Name}' added successfully.");
            return category;
        }

        static Category AddCategoryLoop(CategoryManager categoryManager)
        {
            while (true)
            {
                var category = HandleAddCategory(categoryManager);
                if (category is not null)
                {
                    return category;
                }
            }
        }

        static void RunCategoryMenu(CategoryManager categoryManager, ProductManager productManager)
        {
            while (true)
            {
                var categories = categoryManager.GetAll();

                Utils.Heading("Categories");
                if (categories.Count == 0)
                {
                    Utils.DisplayWarningMessage("No categories exist yet.");
                }
                else
                {
                    for (int i = 0; i < categories.Count; i++)
                    {
                        Console.WriteLine($"- {categories[i].Name}");
                    }
                }
                string[] menuItems =
                    [
                        "Add Category",
                        "Edit Category",
                        "Delete Category",
                        "Back to Main Menu",
                    ];

                Console.WriteLine();
                for (int i = 0; i < menuItems.Length; i++)
                {
                    Console.WriteLine($"{i + 1}. {menuItems[i]}");
                }

                int choice = Utils.ValidateInput(
                    $"Select option (1 - {menuItems.Length}): ",
                    Utils.ValidateIntegerRange(1, menuItems.Length),
                    $"Invalid input, please enter a number between 1 and {menuItems.Length}.");

                switch (choice)
                {
                    case 1: AddCategoryLoop(categoryManager); break;
                    case 2: HandleEditCategory(categoryManager); break;
                    case 3: HandleDeleteCategory(categoryManager, productManager); break;
                    case 4: return;
                }
            }
        }

        static void HandleEditCategory(CategoryManager categoryManager)
        {
            var categories = categoryManager.GetAll();

            Utils.Heading("Categories");
            if (categories.Count == 0)
            {
                Utils.DisplayWarningMessage("No categories to edit.");
                return;
            }

            Category category = Utils.SelectFromList(categories, (c) => c.Name,
                $"Select a category to edit (1 - {categories.Count}): ");

            string newName = Utils.ValidateInput("Enter a new name: ");

            bool success = categoryManager.EditCategory(category.Id, newName);

            if (success)
            {
                Utils.DisplaySuccessMessage("Category updated successfully.");
            }
            else
            {
                Utils.DisplayErrorMessage("A category with that name already exists.");
            }
        }

        static void HandleDeleteCategory(CategoryManager categoryManager, ProductManager productManager)
        {
            var categories = categoryManager.GetAll();

            Utils.Heading("Categories");
            if (categories.Count == 0)
            {
                Utils.DisplayWarningMessage("No categories to delete.");
                return;
            }

            Category category = Utils.SelectFromList(categories, (c) => c.Name,
                $"Select a category to edit (1 - {categories.Count}): ");
            var affectedProducts = productManager.SearchByCategory(category.Id);

            if (affectedProducts.Count > 0)
            {
                Utils.DisplayErrorMessage($"Cannot delete '{category.Name}': {affectedProducts.Count} " +
                    $"{Utils.Pluralize(affectedProducts.Count, "product", "products")} use it");

                foreach (var product in affectedProducts)
                {
                    Console.WriteLine($"{product.Name}");
                }
                return;
            }

            bool confirmed = Utils.Confirm(
                    $"Are you sure you want to delete \"{category.Name}\". This cannot be undone.");

            if (!confirmed)
            {
                Utils.DisplayWarningMessage("Deletion cancelled.");
                return;
            }

            bool success = categoryManager.DeleteCategory(category.Id);

            if (success)
            {
                Utils.DisplaySuccessMessage($"Category \"{category.Name}\" deleted successfully.");
            }
            else
            {
                Utils.DisplayErrorMessage("Something went wrong, could not delete category.");
            }

        }

        static void HandleAddProduct(ProductManager productManager, CategoryManager categoryManager)
        {
            Utils.Heading("Add product");

            string name = Utils.ValidateInput("Enter product name: ");
            decimal price = Utils.ValidateInput(
                "Enter product price: ",
                Utils.ValidatePositiveDecimal(),
                "Input must be a decimal (or whole) number.");

            Category category = SelectOrCreateCategory(categoryManager);

            Product product = productManager.AddProduct(name, price, category.Id);

            Utils.DisplaySuccessMessage($"Product \"{product.Name}\" added to \"{category.Name}\".");
        }

        static void HandleEditProduct(ProductManager productManager, CategoryManager categoryManager)
        {
            var products = productManager.GetAll();

            Utils.Heading("Edit product");
            if (products.Count == 0)
            {
                Utils.DisplayWarningMessage("No products to edit.");
                return;
            }

            Product product = Utils.SelectFromList(
                products,
                p => $"{p.Name} - {Utils.FormatPrice(p.Price)}",
                $"Select product to edit (1 - {products.Count}): ");

            while (true)
            {
                Console.WriteLine($"\nEditing: {product.Name}");
                string[] menuItems = ["Change name", "Change price", "Change category", "Done"];

                for (int i = 0; i < menuItems.Length; i++)
                {
                    Console.WriteLine($"{i + 1}. {menuItems[i]}");
                }

                int choice = Utils.ValidateInput(
                    $"Select option (1 - {menuItems.Length}):",
                    Utils.ValidateIntegerRange(1, menuItems.Length),
                    $"Invalid input, please enter a number between 1 and {menuItems.Length}."
                    );

                switch (choice)
                {
                    case 1: HandleChangeName(productManager, product); break;
                    case 2: HandleChangePrice(productManager, product); break;
                    case 3: HandleChangeCategory(productManager, product, categoryManager); break;
                    case 4: return;
                }

            }
        }

        static void HandleChangeName(ProductManager productManager, Product product)
        {
            Utils.Heading("Change name");

            string newName = Utils.ValidateInput("Enter a new name: ");
            bool success = productManager.UpdateName(product.Id, newName);

            if (success)
            {
                Utils.DisplaySuccessMessage("Name updated successfully.");
            }
            else
            {
                Utils.DisplayErrorMessage("Something went wrong, name was not updated.");
            }
        }

        static void HandleChangePrice(ProductManager productManager, Product product)
        {
            Utils.Heading("Change price");

            decimal newPrice = Utils.ValidateInput(
                "Enter a new price: ",
                Utils.ValidatePositiveDecimal(),
                "Input must be a decimal (or whole) number.");

            bool success = productManager.UpdatePrice(product.Id, newPrice);

            if (success)
            {
                Utils.DisplaySuccessMessage("Price updated successfully.");
            }
            else
            {
                Utils.DisplayErrorMessage("Something went wrong, price was not updated.");
            }
        }

        static void HandleChangeCategory(
            ProductManager productManager, Product product, CategoryManager categoryManager)
        {
            Utils.Heading("Change category");

            Category newCategory = SelectOrCreateCategory(categoryManager);
            bool success = productManager.UpdateCategoryId(product.Id, newCategory.Id);

            if (success)
            {
                Utils.DisplaySuccessMessage("Category updated successfully.");
            }
            else
            {
                Utils.DisplayErrorMessage("Something went wrong, category was not updated.");
            }
        }

        static void HandleDeleteProduct(ProductManager productManager)
        {
            Utils.Heading("Delete product");

            var products = productManager.GetAll();

            if (products.Count == 0)
            {
                Utils.DisplayWarningMessage("No products to delete.");
            }

            Product product = Utils.SelectFromList(products, p => $"{p.Name} - {Utils.FormatPrice(p.Price)}");

            bool confirmed = Utils.Confirm(
                $"Are you sure you want to delete \"{product.Name}\". This cannot be undone.");

            if (!confirmed)
            {
                Utils.DisplayWarningMessage("Deletion cancelled.");
                return;
            }

            bool success = productManager.RemoveProduct(product.Id);

            if (success)
            {
                Utils.DisplaySuccessMessage($"Product \"{product.Name}\" deleted successfully.");
            }
            else
            {
                Utils.DisplayErrorMessage("Something went wrong, could not delete product.");
            }
        }

        static void HandleSearchProduct(ProductManager productManager, CategoryManager categoryManager)
        {
            Utils.Heading("Search product");

            string[] menuItems = ["Search by name", "Search by category"];

            for (int i = 0; i < menuItems.Length; i++)
            {
                Console.WriteLine($"{i + 1}. {menuItems[i]}");
            }

            int choice = Utils.ValidateInput(
                $"Select option (1 - {menuItems.Length}): ",
                Utils.ValidateIntegerRange(1, menuItems.Length),
                $"Invalid input, please enter a number between 1 and {menuItems.Length}.");

            List<Product> results;

            if (choice == 1)
            {
                string term = Utils.ValidateInput("Enter product name to search for: ");
                results = productManager.SearchByName(term);
            }
            else
            {
                var categories = categoryManager.GetAll();

                if (categories.Count == 0)
                {
                    Utils.DisplayWarningMessage("No categories exists yet.");
                    return;
                }

                Category category = Utils.SelectFromList(
                    categories, c => c.Name, $"Select category (1 - {categories.Count}): ");
                results = productManager.SearchByCategory(category.Id);
            }

            if (results.Count == 0)
            {
                Utils.DisplayWarningMessage("No products found.");
                return;
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nFound {results.Count} " +
                $"{Utils.Pluralize(results.Count, "product", "products")}");
            foreach (var product in results)
            {
                Console.WriteLine($"{product.Name} - {Utils.FormatPrice(product.Price)}");
            }
            Console.ResetColor();
        }

        static void HandleSaveData(string path, CategoryManager categoryManager, ProductManager productManager)
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

        static void HandleLoadData(string path, CategoryManager categoryManager, ProductManager productManager)
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

        static void HandleResetData(ProductManager productManager, CategoryManager categoryManager)
        {
            Utils.Heading("Reset data");

            string[] menuItems = ["Reset products only", "Reset products and categories", "Cancel"];

            for (int i = 0; i < menuItems.Length; i++)
            {
                Console.WriteLine($"{i + 1}. {menuItems[i]}");
            }

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

        static void HandleExit(string path, CategoryManager categoryManager, ProductManager productManager)
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
