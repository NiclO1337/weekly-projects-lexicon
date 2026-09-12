using System;
using System.Collections.Generic;
using System.Text;

namespace Week37
{
    internal static class OutputTracker
    {
        private static bool hasWritten;

        public static bool HasWritten
        {
            get => hasWritten;
            set => hasWritten = value;
        }

        public static void Install()
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
}
