using System.Globalization;

namespace AssetTracker.ConsoleApp.UI;

/// <summary>How dates and amounts are shown, in one place.</summary>
internal static class Format
{
    internal const string DateFormat = "yyyy-MM-dd";

    // A space between thousands ("14 990"), so a shown amount can't be mistaken for a decimal
    // comma, which price input accepts.
    private static readonly NumberFormatInfo s_amountFormat = new() { NumberGroupSeparator = " " };

    internal static string Date(DateOnly date) => date.ToString(DateFormat, CultureInfo.InvariantCulture);

    /// <summary>A whole number with thousand separators, e.g. "14 990". The currency code is shown separately.</summary>
    internal static string Amount(decimal amount) =>
        Math.Round(amount, 0, MidpointRounding.AwayFromZero).ToString("N0", s_amountFormat);
}
