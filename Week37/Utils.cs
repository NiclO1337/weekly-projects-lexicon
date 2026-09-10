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

        public static void DisplayErrorMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void DisplaySuccessMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void DisplayWarningMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(message);
            Console.ResetColor();
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

            int choice = ValidateInput(prompt, ValidateIntegerRange(1, items.Count));
            return items[choice - 1];
        }
    }
}
