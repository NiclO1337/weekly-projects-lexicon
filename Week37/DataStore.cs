using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Week37
{
    /// <summary>Handles reading and writing the full application state to a JSON file.</summary>
    internal class DataStore
    {
        private static readonly JsonSerializerOptions options = new() { WriteIndented = true };

        public static void Save(string path, CategoryManager categoryManager, ProductManager productManager)
        {
            AppData data = new()
            {
                Categories = categoryManager.GetAll(),
                Products = productManager.GetAll(),
            };

            string json = JsonSerializer.Serialize(data, options);
            File.WriteAllText(path, json);
        }

        public static bool Load(string path, CategoryManager categoryManager, ProductManager productManager)
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

            AppData? data = JsonSerializer.Deserialize<AppData>(json, options);
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
