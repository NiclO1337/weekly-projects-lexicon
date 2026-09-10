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

    }
}
