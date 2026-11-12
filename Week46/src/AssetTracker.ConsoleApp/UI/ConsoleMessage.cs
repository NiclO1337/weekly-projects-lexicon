using System.Globalization;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace AssetTracker.ConsoleApp.UI;

/// <summary>
/// Owns every coloured piece of output, drawn by Spectre.Console. Text is written as
/// <see cref="Text"/> widgets rather than markup, so data containing <c>[</c> or <c>]</c> can
/// never be misread as a style tag; the one markup string (the heading) is escaped.
/// </summary>
internal static class ConsoleMessage
{
    internal static void DisplayErrorMessage(string message) => WriteColoredLine(message, Theme.Error);

    internal static void DisplaySuccessMessage(string message) => WriteColoredLine(message, Theme.Success);

    internal static void DisplayWarningMessage(string message) => WriteColoredLine(message, Theme.Warning);

    /// <summary>
    /// Writes <paramref name="text"/> in <paramref name="color"/> without a trailing newline, so
    /// it can be combined with plain-colour text on the same line.
    /// </summary>
    internal static void WriteColored(string text, Color color) => AnsiConsole.Write(new Text(text, new Style(color)));

    /// <summary>
    /// Writes <paramref name="text"/> in <paramref name="color"/> followed by a newline.
    /// </summary>
    internal static void WriteColoredLine(string text, Color color)
    {
        WriteColored(text, color);
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Writes <paramref name="text"/> in <paramref name="color"/> with <see cref="SlowConsole"/>'s
    /// typewriter effect. <see cref="SlowConsole"/> owns the pacing; this only colours each character.
    /// </summary>
    internal static void WriteColoredSlow(string text, Color color)
    {
        var style = new Style(color);
        SlowConsole.TypeTextSlow(text, c => AnsiConsole.Write(new Text(c.ToString(), style)));
    }

    /// <summary>
    /// Writes a blank line. Exists so callers outside <c>UI/</c> never need to call
    /// <see cref="Console.WriteLine()"/> directly.
    /// </summary>
    internal static void NewLine() => AnsiConsole.WriteLine();

    /// <summary>Clears the screen and writes <paramref name="title"/> as a left-justified rule.</summary>
    internal static void Heading(string title)
    {
        ClearScreen();
        var titleCase = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(title.ToLowerInvariant());
        var rule = new Rule($"[{Theme.Heading.ToMarkup()}]{Markup.Escape(titleCase)}[/]")
            .LeftJustified()
            .RuleStyle(new Style(Theme.Heading));

        WriteFramed(rule);
    }

    /// <summary>Clears the screen and writes the boxed banner shown at the top of the main menu.</summary>
    internal static void MainHeading(string title)
    {
        ClearScreen();
        var banner = new Panel(new Text(title, new Style(Theme.Accent)))
            .Border(BoxBorder.Double)
            .BorderStyle(new Style(Theme.Accent))
            .Padding(3, 0);

        WriteFramed(banner);
    }

    /// <summary>Writes <paramref name="widget"/> with a blank line above and below it.</summary>
    private static void WriteFramed(IRenderable widget)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(widget);
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Clears the visible screen and the terminal's scrollback. <see cref="Console.Clear"/>
    /// alone only wipes the current viewport — terminals such as Windows Terminal keep prior
    /// screens in a separate scrollback buffer, which is what's left over when the user scrolls
    /// up after navigating menus. The VT sequence clears that history too. Skipped when output
    /// is redirected (piped or scripted runs): there is no screen, and <see cref="Console.Clear"/>
    /// would throw <see cref="IOException"/>.
    /// </summary>
    private static void ClearScreen()
    {
        if (Console.IsOutputRedirected)
        {
            return;
        }

        Console.Clear();
        Console.Write("\x1b[3J");
    }
}
