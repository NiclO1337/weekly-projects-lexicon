using Spectre.Console;

namespace AssetTracker.ConsoleApp.UI;

/// <summary>
/// Every colour the UI uses, defined once. Declared as <see cref="ConsoleColor"/> values and
/// converted with <see cref="Color.FromConsoleColor"/>, so they render as the terminal's own
/// palette colours — the same ones Week38 and the Milestone project used. (Spectre's named
/// colours such as <c>Color.DarkRed</c> are different shades.)
/// </summary>
internal static class Theme
{
    /// <summary>Intro art and text, main banner, table headers.</summary>
    internal static Color Accent { get; } = Color.FromConsoleColor(ConsoleColor.DarkYellow);

    /// <summary>Screen headings.</summary>
    internal static Color Heading { get; } = Color.FromConsoleColor(ConsoleColor.DarkCyan);

    internal static Color Success { get; } = Color.FromConsoleColor(ConsoleColor.Green);

    internal static Color Warning { get; } = Color.FromConsoleColor(ConsoleColor.Yellow);

    internal static Color Error { get; } = Color.FromConsoleColor(ConsoleColor.DarkRed);
}
