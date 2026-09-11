namespace Week37
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //CategoryManager categoryManager = new();
            //categoryManager.AddCategory("Fruit");
            //SelectOrCreateCategory(categoryManager);
            decimal price = 19.00m;
            Console.WriteLine(Math.Floor(price) == price);
        }
        static void DisplayMainMenu()
        {
            Console.WriteLine("\n==============================");
            Console.WriteLine("   PRODUCT MANAGEMENT SYSTEM");
            Console.WriteLine("==============================\n");

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
                "Manage Categories",
                "Exit"
            ];

            for (int i = 0; i < menuItems.Length; i++)
            {
                Console.WriteLine($"{i + 1}. {menuItems[i]}");
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
                    $"Select category (1 - {categories.Count}):", 
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

        static void RunCategoryManager(CategoryManager categoryManager, ProductManager productManager)
        {
            while (true)
            {
                var categories = categoryManager.GetAll();

                Console.WriteLine("\n===== CATEGORIES =====\n");
                if (categories.Count == 0)
                {
                    Utils.DisplayWarningMessage("No categories exist yet.");
                }
                else
                {
                    for (int i = 0; i < categories.Count; i++)
                    {
                        Console.WriteLine($"{i + 1}. {categories[i].Name}");
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

            Console.WriteLine("\n===== CATEGORIES =====\n");
            if (categories.Count == 0)
            {
                Utils.DisplayWarningMessage("No categories to edit.");
                return;
            }

            Category category = Utils.SelectFromList(categories, (c) => c.Name,
                $"Select a category to edit (1 - {categories.Count}):");
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

            Console.WriteLine("\n===== CATEGORIES =====\n");
            if (categories.Count == 0)
            {
                Utils.DisplayWarningMessage("No categories to delete.");
                return;
            }

            Category category = Utils.SelectFromList(categories, (c) => c.Name,
                $"Select a category to edit (1 - {categories.Count}):");
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
            string name = Utils.ValidateInput("Enter product name: ");
            decimal price = Utils.ValidateInput(
                "Enter product price: ",
                Utils.ValidatePositiveDecimal(),
                "Input must be a decimal (or whole) number.");

            Category category = SelectOrCreateCategory(categoryManager);

            Product product = productManager.AddProduct(name, price, category.Id);

            Utils.DisplaySuccessMessage($"Product \"{product.Name}\n added to \"{category.Name}\".");
        }

        static void HandleEditProduct(ProductManager productManager, CategoryManager categoryManager)
        {
            var products = productManager.GetAll();

            Console.WriteLine("\n===== PRODUCTS =====\n");
            if (products.Count == 0)
            {
                Utils.DisplayWarningMessage("No products to edit.");
                return;
            }

            Product product = Utils.SelectFromList(
                products, 
                p => $"{p.Name} - {Utils.FormatPrice(p.Price)}", 
                "Select product to edit: ");

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

            static void HandleChangeName(ProductManager productManager, Product product)
            {
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

                    Category category = Utils.SelectFromList(categories, c => c.Name, "Select category: ");
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
                    Console.WriteLine($"- {product.Name} - {Utils.FormatPrice(product.Price)}");
                }
                Console.ResetColor();
            }
        }
    }
}
