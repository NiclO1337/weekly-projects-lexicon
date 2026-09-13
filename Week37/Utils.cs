using System.Globalization;

namespace Week37
{
    internal static class Utils
    {
        // Explicit Swedish style format: comma decimal, space thousand separator.
        // Not tied to CultureInto.CurrentCulture, so behaviour is identical on every machine.
        private static readonly NumberFormatInfo SwedishNumberFormat = new()
        {
            NumberDecimalSeparator = ",",
            NumberGroupSeparator = " ",
            NumberDecimalDigits = 2,
        };

        internal const int MaxNameLength = 32;

        internal static string ValidateInput(string prompt, bool allowCancel = false)
        {
            while (true)
            {
                Console.Write("\n" + prompt);
                string? input = Console.ReadLine();

                if (allowCancel && IsCancel(input))
                {
                    throw new UserCancelledException();
                }

                if (string.IsNullOrWhiteSpace(input))
                {
                    DisplayErrorMessage("Input can not be empty.");
                    continue;
                }

                if (input.Length > MaxNameLength)
                {
                    DisplayErrorMessage($"Input can be maximum {MaxNameLength} characters.");
                    continue;
                }

                return input;
            }
        }

        internal static T ValidateInput<T>(
            string prompt,
            Func<string, (bool isValid, T result)> validator,
            string errorMessage = "Invalid input, try again.",
            bool allowCancel = false)
        {
            while (true)
            {
                Console.Write("\n" + prompt);
                string? input = Console.ReadLine();

                if (allowCancel && IsCancel(input))
                {
                    throw new UserCancelledException();
                }

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

        private static bool IsCancel(string? input)
        {
            return input is not null && input.Trim().Equals("q", StringComparison.OrdinalIgnoreCase);
        }

        internal static Func<string, (bool isValid, int result)> ValidateIntegerRange(int min, int max)
        {
            return input =>
            {
                if (int.TryParse(input, out int value) && value >= min && value <= max)
                    return (true, value);
                return (false, 0);
            };
        }

        internal static Func<string, (bool isValid, decimal result)> ValidatePositiveDecimal()
        {
            const decimal MaxPrice = 150_000_000m;

            return input =>
            {
                if (decimal.TryParse(
                    input,
                    NumberStyles.Number, // Allows optional thousand separators + decimal point/comma
                    SwedishNumberFormat,
                    out decimal value)
                && value > 0 && value <= MaxPrice)
                {
                    return (true, value);
                }
                return (false, 0);
            };
        }

        internal static void DisplayErrorMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        internal static void DisplaySuccessMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        internal static void DisplayWarningMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        internal static void DisplayNumberedList<T>(IReadOnlyList<T> items, Func<T, string> display)
        {
            for (int i = 0; i < items.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {display(items[i])}");
            }
        }

        internal static void DisplayNumberedList(IReadOnlyList<string> items)
        {
            DisplayNumberedList(items, item => item);
        }

        internal static T SelectFromList<T>(
            List<T> items,
            Func<T, string> display,
            string prompt = "Select an option: ",
            bool allowCancel = false)
        {
            Utils.DisplayNumberedList(items, display);

            int choice = ValidateInput(prompt, 
                ValidateIntegerRange(1, items.Count),
                $"Invalid input, please enter a number between 1 and {items.Count}.",
                allowCancel);

            return items[choice - 1];
        }

        internal static string Pluralize(int count, string singular, string plural)
        {
            return count == 1 ? singular : plural;
        }

        internal static string FormatPrice(decimal price)
        {
            bool showDecimals = price < 100 && price != Math.Floor(price);
            bool useGrouping = price >= 10_000;

            // Truncate (round down) to target precision before formatting,
            // so ToString() has nothing left to round.
            decimal truncated = showDecimals
                ? Math.Floor(price * 100) / 100
                : Math.Floor(price);

            // "F" never groups regardless of NumberFormatInfo; "N" always groups.
            // Switching format letter (not just digit count) is what lets us turn
            // grouping on/off independently of decimals.
            string formatSpec = (useGrouping ? "N" : "F") + (showDecimals ? "2" : "0");
            string formatted = truncated.ToString(formatSpec, SwedishNumberFormat);
            return $"{formatted} kr";
        }

        internal static bool Confirm(string message)
        {
            Utils.DisplayWarningMessage(message);
            Console.WriteLine("1. Yes");
            Console.WriteLine("2. No");

            int choice = ValidateInput("Select option (1 - 2): ", ValidateIntegerRange(1, 2), "Invalid input, select 1 or 2.");
            return choice == 1;
        }

        internal static void Heading(string message)
        {
            string title = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(message.ToLower());
            Console.WriteLine($"\n===== {title} =====\n");
        }

        internal static void TryRun(Action action)
        {
            try
            {
                action();
            }
            catch (UserCancelledException)
            {
                DisplayWarningMessage("Cancelled - returning to previous menu.");
            }
        }
    }
}
