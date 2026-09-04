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
            Console.WriteLine("\nEnter products to save them, type \"exit\" to finish.");

            List<string> productList = [];

            ValidateProductInput(productList);

            Console.WriteLine("\nProducts entered:\n");
            Console.Write("- "); Console.WriteLine(String.Join("\n- ", productList));
        }
        private static List<string> ValidateProductInput(List<string> products)
        {
            while (true)
            {
                Console.Write("Product: ");
                string? input = Console.ReadLine();
                string? trimmedInput = input?.Trim();

                if (string.IsNullOrWhiteSpace(trimmedInput))
                {
                    Console.WriteLine("Product name can not be empty.");
                    continue;
                }

                string[] splitInput = trimmedInput.Split("-");
                Console.WriteLine(splitInput.Length);
                foreach (string part in splitInput)
                {
                    Console.WriteLine(part);
                };

                
                if (trimmedInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    return products;
                } else
                {
                    products.Add(trimmedInput);
                }
            }
        }
    }
}
