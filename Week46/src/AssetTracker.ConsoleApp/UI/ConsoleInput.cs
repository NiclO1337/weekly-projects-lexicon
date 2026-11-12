using AssetTracker.Core.Exceptions;

namespace AssetTracker.ConsoleApp.UI;

/// <summary>
/// Reads and validates console input in a re-prompt loop. Never returns invalid data — bad
/// input is a normal event handled here, not an exception thrown at the caller.
/// </summary>
internal static class ConsoleInput
{
    /// <summary>
    /// Prompts for free text of at most <paramref name="maxLength"/> characters. "q" cancels
    /// (<see cref="UserCancelledException"/>); empty input returns <paramref name="currentValue"/>
    /// when one is given, and re-prompts otherwise.
    /// </summary>
    internal static string ValidateInput(string prompt, int maxLength, string? currentValue = null) =>
        ValidateInput(
            prompt,
            input => (input.Length <= maxLength, input),
            $"Input can be maximum {maxLength} characters.",
            hasCurrentValue: currentValue is not null,
            currentValue: currentValue ?? string.Empty);

    /// <summary>
    /// Prompts until <paramref name="validator"/> accepts the input. "q" always cancels
    /// (<see cref="UserCancelledException"/>) — every typed prompt can be backed out of. Menus
    /// use <see cref="SelectMenuOption"/> instead, where "q" is simply invalid and "0" goes back.
    /// </summary>
    internal static T ValidateInput<T>(
        string prompt,
        Func<string, (bool isValid, T result)> validator,
        string errorMessage = "Invalid input, try again.",
        bool hasCurrentValue = false,
        T currentValue = default!) =>
        ReadUntilValid(
            prompt,
            input => IsCancel(input) ? throw new UserCancelledException() : validator(input),
            errorMessage,
            hasCurrentValue,
            currentValue);

    /// <summary>
    /// The shared re-prompt loop. Knows nothing about "q": cancelling is added by
    /// <see cref="ValidateInput{T}"/>, so menu choices read through here directly treat "q" as
    /// just another invalid answer. End of input still throws <see cref="UserCancelledException"/>,
    /// since there is nothing left to re-prompt.
    /// </summary>
    private static T ReadUntilValid<T>(
        string prompt,
        Func<string, (bool isValid, T result)> validator,
        string errorMessage,
        bool hasCurrentValue = false,
        T currentValue = default!)
    {
        while (true)
        {
            Console.Write("\n" + prompt);
            var input = Console.ReadLine();

            if (input is null)
            {
                throw new UserCancelledException();
            }

            if (string.IsNullOrWhiteSpace(input))
            {
                if (hasCurrentValue)
                {
                    return currentValue;
                }

                ConsoleMessage.DisplayErrorMessage("Input can not be empty.");
                continue;
            }

            var (isValid, result) = validator(input);
            if (isValid)
            {
                return result;
            }

            ConsoleMessage.DisplayErrorMessage(errorMessage);
        }
    }

    internal static Func<string, (bool isValid, int result)> ValidateIntegerRange(int min, int max)
    {
        return input =>
        {
            if (int.TryParse(input, out var value) && value >= min && value <= max)
            {
                return (true, value);
            }

            return (false, 0);
        };
    }

    internal static void DisplayNumberedList<T>(IReadOnlyList<T> items, Func<T, string> display)
    {
        for (var i = 0; i < items.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {display(items[i])}");
        }
    }

    internal static void DisplayNumberedList(IReadOnlyList<string> items) => DisplayNumberedList(items, item => item);

    /// <summary>
    /// Displays <paramref name="menuItems"/> numbered 1..N plus a reserved "0" option, and
    /// returns whichever number was chosen. The valid range and the prompt/error text always
    /// match <paramref name="menuItems"/>'s current length — nothing to keep in sync by hand
    /// when items are added or removed.
    /// </summary>
    /// <param name="disabledChoices">
    /// Choices that are numerically valid but currently unavailable, each mapped to the message
    /// shown when picked. Picking one re-prompts in place — same as any other invalid input —
    /// rather than being returned, so the caller never has to handle it as a real result.
    /// </param>
    internal static int SelectMenuOption(
        IReadOnlyList<string> menuItems,
        string zeroLabel,
        IReadOnlyDictionary<int, string>? disabledChoices = null)
    {
        DisplayNumberedList(menuItems);
        Console.WriteLine($"0. {zeroLabel}");

        while (true)
        {
            var choice = ReadUntilValid(
                $"Select option (0 - {menuItems.Count}): ",
                ValidateIntegerRange(0, menuItems.Count),
                $"Invalid input, select a number between 0 and {menuItems.Count}.");

            if (disabledChoices is not null && disabledChoices.TryGetValue(choice, out var message))
            {
                ConsoleMessage.DisplayErrorMessage(message);
                continue;
            }

            return choice;
        }
    }

    /// <summary>
    /// Lists every value of <typeparamref name="T"/> as a menu, flags <paramref name="current"/>
    /// as "(current)", and returns the chosen value — or <paramref name="current"/> unchanged if
    /// "0. Back" was picked.
    /// </summary>
    internal static T SelectEnumOption<T>(T current)
        where T : struct, Enum
    {
        var values = Enum.GetValues<T>();
        var menuItems = values
            .Select(value => value.Equals(current) ? $"{EnumDisplayName(value)} (current)" : EnumDisplayName(value))
            .ToArray();

        var choice = SelectMenuOption(menuItems, "Back");
        return choice == 0 ? current : values[choice - 1];
    }

    /// <summary>Splits an enum value's PascalCase name into lower-cased words, e.g. <c>IncomesOnly</c> → "Incomes only".</summary>
    internal static string EnumDisplayName<T>(T value)
        where T : struct, Enum
    {
        var name = value.ToString();
        return string.Concat(name.Select((c, i) => i > 0 && char.IsUpper(c) ? $" {char.ToLowerInvariant(c)}" : c.ToString()));
    }

    /// <summary>
    /// Blocks until the user presses a key. Falls back to <see cref="Console.ReadLine"/> when
    /// input is redirected (piped/scripted input), since <see cref="Console.ReadKey()"/> throws
    /// <see cref="InvalidOperationException"/> in that case.
    /// </summary>
    internal static void Pause()
    {
        Console.Write("\nPress any key to continue...");

        if (Console.IsInputRedirected)
        {
            Console.ReadLine();
        }
        else
        {
            Console.ReadKey(intercept: true);
        }

        Console.WriteLine();
    }

    internal static bool Confirm(string message)
    {
        ConsoleMessage.DisplayWarningMessage(message);
        Console.WriteLine("1. Yes");
        Console.WriteLine("2. No");

        var choice = ReadUntilValid("Select option (1 - 2): ", ValidateIntegerRange(1, 2), "Invalid input, select 1 or 2.");
        return choice == 1;
    }

    /// <summary>
    /// Runs a menu action that handles its own pausing (e.g. a screen with its own loop). Only a
    /// cancel or a database failure pauses here, so its message can be read before the calling
    /// menu clears the screen.
    /// </summary>
    internal static void TryRun(Action action) => TryRun(() =>
    {
        action();
        return false;
    });

    /// <summary>
    /// Runs a menu action that reports whether it actually did anything, then pauses before
    /// returning — unless it quietly backed out early (returned <see langword="false"/>) without
    /// ever reaching a step a user could cancel out of with "q". A "q" cancel
    /// (<see cref="UserCancelledException"/>) or a database failure (<see cref="DataStoreException"/>)
    /// shows a message and always pauses, returning to the calling menu instead of crashing.
    /// </summary>
    internal static void TryRun(Func<bool> action)
    {
        var shouldPause = true;
        try
        {
            shouldPause = action();
        }
        catch (UserCancelledException)
        {
            ConsoleMessage.DisplayWarningMessage("Cancelled - returning to previous menu.");
        }
        catch (DataStoreException ex)
        {
            ConsoleMessage.DisplayErrorMessage(ex.Message);
        }

        if (shouldPause)
        {
            Pause();
        }
    }

    private static bool IsCancel(string input) => input.Trim().Equals("q", StringComparison.OrdinalIgnoreCase);
}
