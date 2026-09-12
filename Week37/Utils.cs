namespace Week37
{
    internal class Utils
    {
        public static string ValidateInput(string prompt)
        {
            while (true)
            {
                Console.Write("\n" + prompt);
                string? input = Console.ReadLine();

                if (!string.IsNullOrWhiteSpace(input))
                {
                    return input;
                }
                DisplayErrorMessage("Input can not be empty.");
            }
        }

        public static T ValidateInput<T>(
            string prompt,
            Func<string, (bool isValid, T result)> validator,
            string errorMessage = "Invalid input, try again.")
        {
            while (true)
            {
                Console.Write("\n" + prompt);
                string? input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                {
                    DisplayErrorMessage("Input can not be empty.");
                    continue;
                }

                var (isValid, result) = validator(input);
                if (isValid)
                {
                    return result;
                }

                DisplayErrorMessage(errorMessage);
            }
        }

        public static Func<string, (bool isValid, int result)> ValidateIntegerRange(int min, int max)
        {
            return input =>
            {
                if (int.TryParse(input, out int value) && value >= min && value <= max)
                    return (true, value);
                return (false, 0);
            };
        }

        public static Func<string, (bool isValid, decimal result)> ValidatePositiveDecimal()
        {
            return input =>
            {
                if (decimal.TryParse(input, out decimal value) && value >= 0)
                    return (true, value);
                return (false, 0);
            };
        }

        public static void DisplayErrorMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
            Console.Write("\nPress any key to continue...\n");
            Console.ReadKey();
        }

        public static void DisplaySuccessMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
            Console.Write("\nPress any key to continue...\n");
            Console.ReadKey();
        }

        public static void DisplayWarningMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(message);
            Console.ResetColor();
            Console.Write("\nPress any key to continue...\n");
            Console.ReadKey();
        }
        public static T SelectFromList<T>(
            List<T> items,
            Func<T, string> display,
            string prompt = "Select an option: ")
        {
            for (int i = 0; i < items.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {display(items[i])}");
            }

            int choice = ValidateInput(prompt, 
                ValidateIntegerRange(1, items.Count),
                $"Invalid input, please enter a number between 1 and {items.Count}.");

            return items[choice - 1];
        }

        public static string Pluralize(int count, string singular, string plural)
        {
            return count == 1 ? singular : plural;
        }

        public static string FormatPrice(decimal price)
        {
            return price == Math.Floor(price) ? $"{price:F0} kr" : $"{price:F2} kr";
        }

        public static bool Confirm(string message)
        {
            Utils.DisplayWarningMessage(message);
            Console.WriteLine("1. Yes");
            Console.WriteLine("2. No");

            int choice = ValidateInput("Select option: ", ValidateIntegerRange(1, 2), "Invalid input, select 1 or 2.");
            return choice == 1;
        }

        public static void Heading(string message)
        {
            string title = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(message.ToLower());
            Console.WriteLine($"\n===== {title} =====\n");
        }
    }
}
