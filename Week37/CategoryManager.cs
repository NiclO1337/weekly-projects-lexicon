using System;
using System.Collections.Generic;
using System.Text;

namespace Week37
{
    internal class CategoryManager
    {
        private List<Category> categories = [];
        private int nextId = 1;

        public Category? AddCategory(string name)
        {
            if (NameExists(name))
            {
                return null;
            }

            Category category = new(nextId, name);
            categories.Add(category);
            nextId++;
            return category;
        }

        public bool EditCategory(int id, string newName)
        {
            Category? category = GetById(id);
            if (category is null || NameExists(newName))
            {
                return false;
            }
            category.Name = newName;
            return true;
        }

        public bool DeleteCategory(int id)
        {
            Category? category = GetById(id);
            if (category is null)
            {
                return false;
            }
            categories.Remove(category);
            return true;
        }

        public Category? GetById(int id)
        {
            return categories.FirstOrDefault(c => c.Id == id);
        }

        public bool NameExists(string name)
        {
            return categories.Any(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public List<Category> GetAll()
        {
            return categories;
        }
        public void LoadAll(List<Category> loadedCategories)
        {
            categories = loadedCategories;
            nextId = categories.Count > 0 ? categories.Max(c => c.Id) + 1 : 1;
        }
    }
}
