using System;
using System.Collections.Generic;
using System.Text;

namespace Week37
{
    internal class Product(int id, string name, decimal price, int categoryId)
    {
        public int Id { get; set; } = id;
        public string Name { get; set; } = name;
        public decimal Price { get; set; } = price;
        public int CategoryId { get; set; } = categoryId;
    }
}
