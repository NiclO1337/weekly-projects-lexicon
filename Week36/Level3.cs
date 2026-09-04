using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Week36
{
    internal class Level3
    {
        public static void Run()
        {
            Console.WriteLine("\nEnter products to save them, type \"exit\" to finish.\n" +
                "\nValid product format is LETTERS-NUMBERS, one dash is requred between the letters and number.\n" +
                "There can be between 1 and 5 letters and numbers must be in range 200 to 500.");

            List<string> productList = [];

            ValidateProductInput(productList);

            productList.Sort();

            Console.WriteLine("\nProducts entered (sorted A-Z):\n");
            Console.Write("- "); Console.WriteLine(String.Join("\n- ", productList));
        }
        private static List<string> ValidateProductInput(List<string> products)
        {
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
                    return products;
                }

                string[]? splitInput = trimmedInput?.Split("-");

                if (splitInput?.Length != 2)
                {
                    Console.WriteLine("Error: Product name requires 1 and only 1 dash ( - ).");
                } 
                else
                {
                    bool isLetters = splitInput[0].All(char.IsLetter);
                    bool isNumbers = int.TryParse(splitInput[1], out int numbers);
                    bool isValid = true;

                    foreach (string part in splitInput)
                    {
                        Console.WriteLine(part);
                    }

                    if (!isLetters)
                    {
                        Console.WriteLine("Error: Left side of the dash ( - ) must only contain letters.");
                        isValid = false;
                    }
                    if (splitInput[0].Length < 1 || splitInput[0].Length > 5)
                    {
                        Console.WriteLine("Error: Must only use between 1 and 5 letters.");
                        isValid = false;
                    }
                    if (!isNumbers)
                    {
                        Console.WriteLine("Error: Right side of the dash ( - ) must only contain numbers.");
                        isValid = false;
                    }
                    if (numbers < 200 || numbers > 500)
                    {
                        Console.WriteLine("Error: Number must be in valid range between 200 and 500.");
                        isValid = false;
                    }

                    if (isValid)
                    {
                        products.Add(trimmedInput!);
                        Console.WriteLine("Successfully added product.");
                    }
                }                
            }
        }
    }
}
