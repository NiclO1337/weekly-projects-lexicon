namespace Week37
{
    internal class Program
    {
        static void Main(string[] args)
        {
            CategoryManager categoryManager = new();
            categoryManager.AddCategory("Fruit");
            SelectOrCreateCategory(categoryManager);
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
                    Utils.ValidateIntegerRange(1, menuItems.Length));

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

            categoryManager.DeleteCategory(category.Id);
            Utils.DisplaySuccessMessage($"Category \"{category.Name}\" deleted successfully.");
        }
    }
}
