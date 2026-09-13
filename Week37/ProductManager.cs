using System;
using System.Collections.Generic;
using System.Text;

namespace Week37
{
    internal class ProductManager(CategoryManager categoryManager)
    {
        private List<Product> products = [];
        private int nextId = 1;
        private readonly CategoryManager categoryManager = categoryManager;

        public Product AddProduct(string name, decimal price, int categoryId)
        {
            Product product = new(nextId, name, price, categoryId);
            products.Add(product);
            nextId++;
            return product;
        }

        public bool UpdateName(int id, string newName)
        {
            Product? product = GetById(id);
            if (product is null)
            {
                return false;
            }

            product.Name = newName;
            return true;
        }

        public bool UpdatePrice(int id, decimal newPrice)
        {
            Product? product = GetById(id);
            if (product is null)
            {
                return false;
            }

            product.Price = newPrice;
            return true;
        }

        public bool UpdateCategoryId(int id, int newCategoryId)
        {
            Product? product = GetById(id);
            if (product is null)
            {
                return false;
            }

            product.CategoryId = newCategoryId;
            return true;
        }

        public bool RemoveProduct(int id)
        {
            Product? product = GetById(id);
            if (product is null)
            {
                return false;
            }

            products.Remove(product);
            return true;
        }

        public void ShowProducts()
        {
            Utils.Heading("Show products");

            if (products.Count == 0)
            {
                Utils.DisplayWarningMessage("No products added yet.");
                return;
            }

            List<Product> sorted = products.OrderBy(p => p.Price).ToList();

            while (true)
            {
                DisplayProductTable(sorted);

                Console.WriteLine();
                string[] menuItems =
                [
                    "Sort: Price Low to High",
                    "Sort: Price High to Low",
                    "Sort: By Category",
                    "Back to Main Menu",
                ];

                Utils.DisplayNumberedList(menuItems);

                int choice = Utils.ValidateInput(
                    $"Select option (1 - {menuItems.Length}): ",
                    Utils.ValidateIntegerRange(1, menuItems.Length),
                    $"Invalid input, please enter a number between 1 and {menuItems.Length}.");

                switch (choice)
                {
                    case 1: sorted = products.OrderBy(p => p.Price).ToList(); break;
                    case 2: sorted = products.OrderByDescending(p => p.Price).ToList(); break;
                    case 3:
                        sorted = products.
                            OrderBy(p => categoryManager.GetById(p.CategoryId)?.Name ?? "Unknown").ToList();
                        break;
                    case 4: OutputTracker.HasWritten = false; return;
                }
            }
        }

        private void DisplayProductTable(List<Product> list)
        {
            int extraPadding = 1;
            int idWidth = Math.Max("ID".Length, products.Max(p => p.Id.ToString().Length)) + extraPadding;
            int nameWidth = Math.Max("Name".Length,
                Math.Min(products.Max(p => p.Name.Length), Utils.MaxNameLength)) + extraPadding;
            int priceWidth = Math.Max("Price".Length,
                products.Max(p => Utils.FormatPrice(p.Price).Length)) + extraPadding;
            int categoryWidth = Math.Max("Category".Length,
                products.Max(p => (categoryManager.GetById(p.CategoryId)?.Name ?? "Unknown").Length));

            string header = $"{"ID".PadRight(idWidth)} | {"Name".PadRight(nameWidth)} | " +
                $"{"Price".PadRight(priceWidth)} | {"Category".PadRight(categoryWidth)}";
            Console.WriteLine(header);
            Console.WriteLine(new string('-', header.Length));

            foreach (Product product in list)
            {
                Category? category = categoryManager.GetById(product.CategoryId);
                string categoryName = category?.Name ?? "Unknown";
                string id = product.Id.ToString().PadRight(idWidth);
                string name = product.Name.PadRight(nameWidth);
                string price = Utils.FormatPrice(product.Price).PadRight(priceWidth);
                Console.WriteLine(
                    $"{id} | {name} | {price} | {categoryName} ");
            }

            Console.WriteLine(new string('-', header.Length));
            Console.WriteLine($"Total Price: {Utils.FormatPrice(CalculateTotal())}");
        }

        public Product? GetById(int id)
        {
            return products.FirstOrDefault(p => p.Id == id);
        }

        public List<Product> GetAll()
        {
            return products;
        }

        public List<Product> SearchByName(string term)
        {
            return products
                .Where(p => p.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public List<Product> SearchByCategory(int categoryId)
        {  
            return products.Where(p => p.CategoryId == categoryId).ToList();
        }

        public void LoadAll(List<Product> loadedProducts)
        {
            products = loadedProducts;
            nextId = products.Count > 0 ? products.Max(p  => p.Id) + 1 : 1;
        }

        public void ClearAll()
        {
            products.Clear();
            nextId = 1;
        }

        public decimal CalculateTotal()
        {
            return products.Sum(p => p.Price);
        }

        public void ShowStatictics()
        {
            Utils.Heading("Show statistics");
            
            if (products.Count == 0)
            {
                Utils.DisplayWarningMessage("No products yet - add products to show statistics.");
                return;
            }

            Product mostExpensive = products.OrderByDescending(p => p.Price).First();
            Product cheapest = products.OrderBy(p => p.Price).First();
            decimal averagePrice = products.Average(p => p.Price);

            Utils.Heading("Statistics");
            Console.WriteLine($"Most Expensive Product:\n" +
                $"{mostExpensive.Name} - {Utils.FormatPrice(mostExpensive.Price)}\n");
            Console.WriteLine($"Cheapest Product:\n" +
                $"{cheapest.Name} - {Utils.FormatPrice(cheapest.Price)}\n");
            Console.WriteLine(
                $"Average Price:\n{Utils.FormatPrice(averagePrice)}\n");

            Console.WriteLine("Products per Category:");
            var CountsByCategory = products
                .GroupBy(p => p.CategoryId)
                .Select(group => new
                {
                    CategoryId = group.Key,
                    Count = group.Count(),
                });

            foreach (var entry in CountsByCategory)
            {
                Category? category = categoryManager.GetById(entry.CategoryId);
                string categoryName = category?.Name ?? "Unknown";
                Console.WriteLine($"{categoryName}: {entry.Count}");
            }
        }
    }
}
