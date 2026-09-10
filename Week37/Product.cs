using System;
using System.Collections.Generic;
using System.Text;

namespace Week37
{
    internal class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }

        public Product(int id, string name, decimal price, int categoryId)
        {
            Id = id;
            Name = name;
            Price = price;
            CategoryId = categoryId;
        }
    }
}
