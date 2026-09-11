using System;
using System.Collections.Generic;
using System.Text;

namespace Week37
{
    internal class ProductManager
    {
        private List<Product> products = [];
        private int nextId = 1;
        private CategoryManager categoryManager;

        public ProductManager(CategoryManager categoryManager)
        {
            this.categoryManager = categoryManager;
        }

        public Product AddProduct(string name, decimal price, int categoryId)
        {
            Product product = new Product(nextId, name, price, categoryId);
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
            if (products.Count == 0)
            {
                Console.WriteLine("No products added yet.");
                return;
            }

            foreach (Product product in products)
            {
                Category? category = categoryManager.GetById(product.CategoryId);
                string categoryName = category?.Name ?? "Unknown";
                Console.WriteLine(
                    $"{product.Id} {product.Name} | {Utils.FormatPrice(product.Price)} | {categoryName} ");
            }
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

        public decimal CalculateTotal()
        {
            return products.Sum(p => p.Price);
        }

        public void ShowStatictics()
        {
            if (products.Count == 0)
            {
                Console.WriteLine("No products yet - add products to show statistics.");
                return;
            }

            Product mostExpensive = products.OrderByDescending(p => p.Price).First();
            Product cheapest = products.OrderBy(p => p.Price).First();
            decimal averagePrice = products.Average(p => p.Price);

            Console.WriteLine("\n===== STATISTICS =====\n");
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
