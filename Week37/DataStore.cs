using System;
using System.Collections.Generic;
using System.Text;

namespace Week37
{
    internal class DataStore
    {
        public void Save(string path, CategoryManager categoryManager, ProductManager productManager)
        {
            AppData data = new()
            {
                Categories = categoryManager.GetAll(),
                Products = productManager.GetAll(),
            };

            string json = System.Text.Json.JsonSerializer.Serialize(data, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }

        public bool Load(string path, CategoryManager categoryManager, ProductManager productManager)
        {
            if (!File.Exists(path))
            {
                return false;
            }

            string json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json))
            { 
                return false; 
            }

            AppData? data = System.Text.Json.JsonSerializer.Deserialize<AppData>(json);
            if (data is null)
            {
                return false;
            }

            categoryManager.LoadAll(data.Categories);
            productManager.LoadAll(data.Products);
            return true;
        }
    }
}
