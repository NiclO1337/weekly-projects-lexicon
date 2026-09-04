namespace Week36
{
    internal class Level4
    {
        private static List<string> productList = [ "XAN-456", "CFD-444", "PSG-333", "AB-234", "SFPU-269" ];

        public static void Run()
        {
            MainMenu();

            Console.WriteLine("\nThank you for using the Product List Manager.\n\n" +
                "      _.-'''''-._\r\n    .'  _     _  '.\r\n   /   (_)   (_)   \\\r\n  |  ,           ,  |   Have a nice day!\r\n  |  \\`.       .`/  |\r\n   \\  '.`'\"\"'\"`.'  /\r\n    '.  `'---'`  .'\r\njgs   '-._____.-'\r\n");
        }

        private static void MainMenu()
        {
            productList.Sort();
            Console.WriteLine();
            string[] menuItems = ["Add product", "View products", "Search products", "Delete product", "Exit application"];

            for (int i = 0; i < menuItems.Length; i++)
            {
                Console.WriteLine(i + 1 + ". " + menuItems[i]);
            }

            while (true)
            {
                Console.Write("\nSelect option: ");
                string? input = Console.ReadLine();
                string? trimmedInput = input?.Trim();

                if (string.IsNullOrWhiteSpace(trimmedInput))
                {
                    Console.WriteLine("Error: Can not be empty. Please select an option.");
                    continue;
                }

                switch (trimmedInput)
                {
                    case "1": AddProduct(); MainMenu(); break;
                    case "2": ViewProducts(); MainMenu(); break;
                    case "3": SearchProducts(); MainMenu(); break;
                    case "4": DeleteProduct(); MainMenu(); break;
                    case "5": break;
                    default: Console.WriteLine("No such option, please try again."); MainMenu(); break;
                }
                break;
            }
        }

        private static void AddProduct()
        {
            Console.WriteLine("\nEnter products to save them, type \"exit\" to go back to main menu.\n" +
                "\nValid product format is LETTERS-NUMBERS, one dash is requred between the letters and number.\n" +
                "There can be between 1 and 5 letters and numbers must be in range 200 to 500.");

            while (true)
            {
                Console.Write("\nProduct: ");
                string? input = Console.ReadLine();
                string? trimmedInput = input?.Trim();

                if (string.IsNullOrWhiteSpace(trimmedInput))
                {
                    Console.WriteLine("Error: Product name can not be empty.");
                    continue;
                }
                else if (trimmedInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
                else if (productList.Any(product => product.Equals(trimmedInput, StringComparison.OrdinalIgnoreCase)))
                {
                    Console.WriteLine("Warning: Product already exists!");
                    continue;
                }

                (bool isValid, string? errorMessage) = ValidateProductInput(trimmedInput);


                if (isValid)
                {
                    productList.Add(trimmedInput!);
                    Console.WriteLine("Successfully added product.");
                }
                else if (!string.IsNullOrEmpty(errorMessage))
                {
                    Console.WriteLine(errorMessage);
                }
            }
        }

        private static (bool, string?) ValidateProductInput(string input)
        {
            string errorMessage = "";
            string[]? splitInput = input?.Split("-");

            if (splitInput?.Length == 1)
            {
                errorMessage += "Error: Product name requires a dash ( - ).";
                return (false, errorMessage);
            }
            else if (splitInput?.Length != 2)
            {
                errorMessage += "Error: Product name requires 1 and only 1 dash ( - ).\n";
            }

            // Validate left side (letters) and right side (numbers)    
            bool isLetters = splitInput![0].All(char.IsLetter) && !string.IsNullOrEmpty(splitInput[0]);
            bool isNumbers = int.TryParse(splitInput[^1], out int numbers); // [^1] gets the last element

            if (!isLetters)
            {
                errorMessage += "Error: Left side of the dash ( - ) must only contain letters.\n";
            }
            if (splitInput[0].Length < 1 || splitInput[0].Length > 5)
            {
                errorMessage += "Error: Must only use between 1 and 5 letters.\n";
            }
            if (!isNumbers)
            {
                errorMessage += "Error: Right side of the dash ( - ) must only contain numbers.\n";
            }
            if (numbers < 200 || numbers > 500)
            {
                errorMessage += "Error: Number must be in valid range between 200 and 500.\n";
            }

            bool isValid = string.IsNullOrEmpty(errorMessage);
            return (isValid, isValid ? null : errorMessage.TrimEnd());
        }
        private static void ViewProducts()
        {
            if (productList.Count == 0) 
            { 
                Console.WriteLine("\nThere are 0 products in the database.");
            }
            else
            {
                Console.WriteLine("\nProducts (sorted A-Z):\n");
                Console.Write("- "); Console.WriteLine(String.Join("\n- ", productList));
            }
            Console.Write("\nPress ENTER to continue...");
            Console.ReadLine();
        }

        private static void SearchProducts()
        {
            while (true)
            {
                Console.Write("\nEnter a product name to search for: ");
                string? searchTerm = Console.ReadLine();
                string trimmedSearchTerm = searchTerm!.Trim();

                if (string.IsNullOrEmpty(trimmedSearchTerm))
                {
                    Console.WriteLine("Error: Can not search for blank input");
                    continue;
                }

                var results = productList.Where(product => product.Contains(trimmedSearchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
                if (results.Count > 0)
                {
                    Console.WriteLine("Found " + results.Count + " products:");
                    foreach (var product in results)
                    {
                        Console.WriteLine("- " + product);
                    }
                }
                else
                {
                    Console.WriteLine("No results was found for: " + searchTerm);
                }
                break;
            }
            
            
            Console.Write("\nPress ENTER to continue...");
            Console.ReadLine();
        }

        private static void DeleteProduct()
        {
            if (productList.Count == 0) 
            {
                Console.WriteLine("\nThere are 0 products in the system.");
            }
            else
            {
                Console.WriteLine("\nThere are " + productList.Count + " products in the system.\n");

                for (int i = 0; i < productList.Count; i++)
                {
                    Console.WriteLine(i + 1 + ". " + productList[i]);
                }

                while (true)
                {
                    Console.Write("\nWarning! This is a destructive action that can not be undone once performed, type \"exit\" to go back to main menu." +
                    "\nWhich product do you wish to delete: ");
                    string? input = Console.ReadLine();
                    string trimmedInput = input!.Trim();

                    if (string.IsNullOrEmpty(trimmedInput))
                    {
                        Console.WriteLine("Error: Can accept a blank input");
                        continue;
                    }
                    else if (trimmedInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }

                    if (int.TryParse(trimmedInput, out int choice) && choice >= 1 && choice <= productList.Count)
                    {
                        string productToDelete = productList[choice - 1];  // Convert to 0-based index
                        productList.RemoveAt(choice - 1);
                        Console.WriteLine("Successfully deleted: " + productToDelete);
                        break;
                    }
                    else
                    {
                        Console.WriteLine("\nError: Please enter a valid number between 1 and " + productList.Count);
                    }
                }
            }
            Console.Write("\nPress ENTER to continue...");
            Console.ReadLine();
        }
    }
}
