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
    // Applied per '\n'-delimited segment in every Write/WriteLine(string) call, so
    // multi-line strings (e.g. "\nPage 1 of 3\n") pace the same as separate WriteLine
    // calls - callers never need to remember to route output through a "slow" helper.
    private const int RowDelayMs = 30;

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
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            hasWritten = true;

            string[] segments = value.Split('\n');
            for (int i = 0; i < segments.Length; i++)
            {
                inner.Write(segments[i]);
                if (i < segments.Length - 1)
                {
                    inner.Write('\n');
                    Thread.Sleep(RowDelayMs);
                }
            }
        }

        public override void Write(char value)
        {
            hasWritten = true;
            inner.Write(value);
        }

        public override void WriteLine(string? value)
        {
            hasWritten = true;

            if (string.IsNullOrEmpty(value))
            {
                inner.WriteLine();
                Thread.Sleep(RowDelayMs);
                return;
            }

            foreach (string line in value.Split('\n'))
            {
                inner.WriteLine(line);
                Thread.Sleep(RowDelayMs);
            }
        }

        public override void WriteLine()
        {
            hasWritten = true;
            inner.WriteLine();
            Thread.Sleep(RowDelayMs);
        }
    }
}
