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

            productList.Sort();

            Console.WriteLine("\nProducts entered (sorted A-Z):\n");
            Console.Write("- "); Console.WriteLine(String.Join("\n- ", productList));
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
                
            bool isLetters = splitInput![0].All(char.IsLetter) && !string.IsNullOrEmpty(splitInput[0]);
            bool isNumbers = int.TryParse(splitInput[^1], out int numbers);
            
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
    }
}
