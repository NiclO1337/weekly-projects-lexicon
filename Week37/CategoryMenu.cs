using System;
using System.Collections.Generic;
using System.Text;

namespace Week37
{
    internal static class CategoryMenu
    {
        internal static void RunCategoryMenu(CategoryManager categoryManager, ProductManager productManager)
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
                Utils.DisplayNumberedList(menuItems);

                int choice = Utils.ValidateInput(
                    $"Select option (1 - {menuItems.Length}): ",
                    Utils.ValidateIntegerRange(1, menuItems.Length),
                    $"Invalid input, please enter a number between 1 and {menuItems.Length}.");

                switch (choice)
                {
                    case 1:
                        Utils.TryRun(
                        () => AddCategoryLoop(categoryManager)
                        ); break;
                    case 2:
                        Utils.TryRun(
                        () => HandleEditCategory(categoryManager)
                        ); break;
                    case 3:
                        Utils.TryRun(
                    () => HandleDeleteCategory(categoryManager, productManager)
                    ); break;
                    case 4: OutputTracker.HasWritten = false; return;
                }
            }
        }

        private static Category? HandleAddCategory(CategoryManager categoryManager)
        {
            Utils.Heading("Add Category");

            string name = Utils.ValidateInput("Enter category name (or \"q\" to quit): ",
                allowCancel: true);

            Category? category = categoryManager.AddCategory(name);

            if (category is null)
            {
                Utils.DisplayErrorMessage("A category with that name already exists.");
                return null;
            }

            Utils.DisplaySuccessMessage($"Category '{category.Name}' added successfully.");
            return category;
        }

        private static Category AddCategoryLoop(CategoryManager categoryManager)
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


        private static void HandleEditCategory(CategoryManager categoryManager)
        {
            var categories = categoryManager.GetAll();

            Utils.Heading("Categories");
            if (categories.Count == 0)
            {
                Utils.DisplayWarningMessage("No categories to edit.");
                return;
            }

            Category category = Utils.SelectFromList(categories, (c) => c.Name,
                $"Select a category to edit (1 - {categories.Count} or \"q\" to quit): ",
                allowCancel: true);

            string newName = Utils.ValidateInput("Enter a new name (or \"q\" to quit): ",
                allowCancel: true);

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

        private static void HandleDeleteCategory(CategoryManager categoryManager, ProductManager productManager)
        {
            var categories = categoryManager.GetAll();

            Utils.Heading("Categories");
            if (categories.Count == 0)
            {
                Utils.DisplayWarningMessage("No categories to delete.");
                return;
            }

            Category category = Utils.SelectFromList(categories, (c) => c.Name,
                $"Select a category to delete (1 - {categories.Count} or \"q\" to quit): ",
                allowCancel: true);

            var affectedProducts = productManager.SearchByCategory(category.Id);

            if (affectedProducts.Count > 0)
            {
                Utils.DisplayErrorMessage($"Cannot delete \"{category.Name}\" {affectedProducts.Count} " +
                    $"{Utils.Pluralize(affectedProducts.Count, "product", "products")} use it:");

                foreach (var product in affectedProducts)
                {
                    Console.WriteLine($"- {product.Name}");
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

        internal static Category SelectOrCreateCategory(CategoryManager categoryManager)
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

                Utils.DisplayNumberedList(categories, c => c.Name);

                int choice = Utils.ValidateInput(
                    $"Select category (1 - {categories.Count}, or \"q\" to quit): ",
                    Utils.ValidateIntegerRange(0, categories.Count),
                    $"Invalid input, please enter a number between 1 and {categories.Count}.",
                    allowCancel: true);

                if (choice == 0)
                {
                    return AddCategoryLoop(categoryManager);
                }

                return categories[choice - 1];
            }
        }
    }
}
