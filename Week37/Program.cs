namespace Week37
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            Utils.DisplayErrorMessage("Hello");
        }
        static void DisplayMainMenu()
        {
            Console.WriteLine("\n==============================");
            Console.WriteLine("   PRODUCT MANAGEMENT SYSTEM");
            Console.WriteLine("==============================\n");

            string[] menuItems =
            [
                "Add Product",
                "Show Products",
                "Search Product",
                "Edit Product",
                "Delete Product",
                "Statistics",
                "Save Data",
                "Load Data",
                "Manage Categories",
                "Exit"
            ];

            for (int i = 0; i < menuItems.Length; i++)
            {
                Console.WriteLine($"{i + 1}. {menuItems[i]}");
            }
        }
        static Category SelectOrCreateCategory(CategoryManager categoryManager)
        {
            while (true)
            {
                var categories = categoryManager.GetAll();

                if (categories.Count == 0)
                {
                    Utils.DisplayWarningMessage("No categories exist yet. Let's add one.");
                    return HandleAddCategory(categoryManager);
                }

                Console.WriteLine("\n0. + Add new category");
                for (int i = 0; i < categories.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {categories[i].Name}");
                }

                int choice = Utils.ValidateInput("Select category: ", Utils.ValidateIntegerRange(0, categories.Count));

                if (choice == 0)
                {
                    return HandleAddCategory(categoryManager);
                }

                return categories[choice - 1];
            }
        }
    }
}
