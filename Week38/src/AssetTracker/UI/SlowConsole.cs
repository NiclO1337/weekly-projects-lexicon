namespace AssetTracker.UI;

internal static class SlowConsole
{
    // TODO: change CharDelayMs to 25
    private const int CharDelayMs = 5;

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
