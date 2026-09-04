using System;
using System.Collections.Generic;
using System.Text;

namespace Week36
{
    internal class Level2
    {
        public static void Run()
        {
            Console.WriteLine("\nEnter products to save them, type \"exit\" to finish.");

            List<string> products = [];

            while (true)
            {
                Console.Write("Product: ");
                string? input = Console.ReadLine();

                if (input?.Trim().ToLower() == "exit")
                {
                    break;
                }
                if (input?.Trim().Length > 0) { products.Add(input.Trim()); }
            }

            products.Sort();

            Console.WriteLine("\nProducts entered:\n");
            Console.Write("- "); Console.WriteLine(String.Join("\n- ", products));
        }
    }
}
