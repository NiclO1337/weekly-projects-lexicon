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
    }
}
