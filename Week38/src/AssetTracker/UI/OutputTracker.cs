using System.Text;

namespace AssetTracker.UI;

/// <summary>
/// Wraps Console.Out to track whether anything has been printed since the flag
/// was last cleared. MainMenu uses this to decide whether to pause with
/// "press any key to continue" — only pausing when a handler actually produced
/// output, instead of after every single menu loop.
/// </summary>
internal static class OutputTracker
{
    private static bool hasWritten;

    internal static bool HasWritten
    {
        get => hasWritten;
        set => hasWritten = value;
    }

    internal static void Install()
    {
        Console.SetOut(new TrackingWriter(Console.Out));
    }

    private class TrackingWriter(TextWriter inner) : TextWriter
    {
        public override Encoding Encoding => inner.Encoding;

        public override void Write(string? value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                hasWritten = true;
            }
            inner.Write(value);
        }

        public override void WriteLine(string? value)
        {
            hasWritten = true;
            inner.WriteLine(value);
        }

        public override void WriteLine()
        {
            hasWritten = true;
            inner.WriteLine();
        }
    }
}
