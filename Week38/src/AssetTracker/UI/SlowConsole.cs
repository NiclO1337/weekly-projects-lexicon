namespace AssetTracker.UI;

internal static class SlowConsole
{
    private const int CharDelayMs = 25;

    internal static void WriteLineSlow(string text, int delayMs = CharDelayMs)
    {
        foreach (char c in text)
        {
            Console.Write(c);
            Thread.Sleep(delayMs);
        }

        Console.WriteLine();
    }
}
