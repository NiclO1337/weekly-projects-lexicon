using System;
using System.Collections.Generic;
using System.Text;

namespace Week37
{
    internal static class ProductMenu
    {
        internal static void HandleAddProduct(ProductManager productManager, CategoryManager categoryManager)
        {
            Utils.Heading("Add product");

            string name = Utils.ValidateInput("Enter product name (or \"q\" to quit): ", allowCancel: true);
            decimal price = Utils.ValidateInput(
                "Enter product price (positive number e.g. 19,90 or \"q\" to quit): ",
                Utils.ValidatePositiveDecimal(),
                "Invalid price. Use a comma for decimals (e.g. 19,90) and keep it under 150 000 000.",
                allowCancel: true);

            Category category = CategoryMenu.SelectOrCreateCategory(categoryManager);

            Product product = productManager.AddProduct(name, price, category.Id);

            Utils.DisplaySuccessMessage($"Product \"{product.Name}\" added to \"{category.Name}\".");
        }

        internal static void HandleEditProduct(ProductManager productManager, CategoryManager categoryManager)
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
                $"Select product to edit (1 - {products.Count} or \"q\" to quit): ",
                allowCancel: true);

            while (true)
            {
                Console.WriteLine($"\nEditing: {product.Name}");
                string[] menuItems = ["Change name", "Change price", "Change category", "Back to Main Menu"];

                Utils.DisplayNumberedList(menuItems);

                int choice = Utils.ValidateInput(
                    $"Select option (1 - {menuItems.Length}): ",
                    Utils.ValidateIntegerRange(1, menuItems.Length),
                    $"Invalid input, please enter a number between 1 and {menuItems.Length}."
                    );

                switch (choice)
                {
                    case 1:
                        Utils.TryRun(
                        () => HandleChangeName(productManager, product)
                        ); break;
                    case 2:
                        Utils.TryRun(
                        () => HandleChangePrice(productManager, product)
                        ); break;
                    case 3:
                        Utils.TryRun(
                        () => HandleChangeCategory(productManager, product, categoryManager)
                        ); break;
                    case 4: OutputTracker.HasWritten = false; return;
                }
            }
        }

        private static void HandleChangeName(ProductManager productManager, Product product)
        {
            Utils.Heading("Change name");

            string newName = Utils.ValidateInput("Enter a new name (or \"q\" to quit): ",
                allowCancel: true);
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

        private static void HandleChangePrice(ProductManager productManager, Product product)
        {
            Utils.Heading("Change price");

            decimal newPrice = Utils.ValidateInput(
                "Enter new price (positive number e.g. 19,90 or \"q\" to quit): ",
                Utils.ValidatePositiveDecimal(),
                "Invalid price. Use a comma for decimals (e.g. 19,90) and keep it under 150 000 000.",
                allowCancel: true);

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

        private static void HandleChangeCategory(
            ProductManager productManager, Product product, CategoryManager categoryManager)
        {
            Utils.Heading("Change category");

            Category newCategory = CategoryMenu.SelectOrCreateCategory(categoryManager);
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

        internal static void HandleDeleteProduct(ProductManager productManager)
        {
            Utils.Heading("Delete product");

            var products = productManager.GetAll();

            if (products.Count == 0)
            {
                Utils.DisplayWarningMessage("No products to delete.");
                return;
            }

            Product product = Utils.SelectFromList(products, p => $"{p.Name} - {Utils.FormatPrice(p.Price)}",
                $"Select a product to delete (1 - {products.Count} or \"q\" to quit): ",
                allowCancel: true);

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

        internal static void HandleSearchProduct(ProductManager productManager, CategoryManager categoryManager)
        {
            Utils.Heading("Search product");

            string[] menuItems = ["Search by name", "Search by category"];

            Utils.DisplayNumberedList(menuItems);

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
            Console.WriteLine($"\nFound {results.Count} " +
                $"{Utils.Pluralize(results.Count, "product", "products")}");

            Console.ForegroundColor = ConsoleColor.Green;
            foreach (var product in results)
            {
                Console.WriteLine($"{product.Name} - {Utils.FormatPrice(product.Price)}");
            }
            Console.ResetColor();
        }
    }
}
