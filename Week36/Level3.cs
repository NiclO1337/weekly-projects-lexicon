using System;
using System.Collections.Generic;
using System.Text;

namespace Week36
{
    internal class Level3
    {
        public static void Run()
        {
            Console.WriteLine("\nEnter products to save them, type \"exit\" to finish.");

            List<string> products = [];

            

            Console.WriteLine("\nProducts entered:\n");
            Console.Write("- "); Console.WriteLine(String.Join("\n- ", products));
        }
        private static string ValidateInput(string prompt, Func<string, bool> isValid)
        {
            while (true)
            {
                Console.Write(prompt);
                string? input = Console.ReadLine();
                string? trimmedInput = input?.Trim();

                if (isValid(trimmedInput))
                {
                    return trimmedInput;
                }

                Console.WriteLine("Invalid input. Please try again.");
            }
        }
    }
}
