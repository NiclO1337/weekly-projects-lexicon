using System.Globalization;
using System.Text.RegularExpressions;
using AssetTracker.Core.Models;

namespace AssetTracker.ConsoleApp.UI;

/// <summary>
/// Parsers for <see cref="ConsoleInput.ValidateInput{T}"/>. They mirror the domain's rules so bad
/// input is re-prompted here instead of reaching <see cref="Asset"/> and throwing.
/// </summary>
internal static partial class InputValidators
{
    /// <summary>
    /// A price greater than 0 and at most <see cref="Asset.MaxPurchasePrice"/>. Accepts plain
    /// digits or thousands grouped by a space, "." or ",", and "." or "," before 1–2 decimals:
    /// "14990", "14 990", "15.000,37" and "15,000.37" all work. Anything ambiguous re-prompts.
    /// </summary>
    internal static Func<string, (bool isValid, decimal result)> Price() => input =>
    {
        var match = PricePattern().Match(input.Trim());
        var groupSeparator = match.Groups["group"];
        var decimalSeparator = match.Groups["decimal"];

        // "15.000.37" matches the pattern but has no clear meaning: the same character can't
        // separate thousands and decimals.
        if (!match.Success || (groupSeparator.Success && decimalSeparator.Success && groupSeparator.Value == decimalSeparator.Value))
        {
            return (false, 0);
        }

        var wholePart = string.Concat(match.Groups["whole"].Value.Where(char.IsAsciiDigit));
        var number = decimalSeparator.Success ? $"{wholePart}.{match.Groups["fraction"].Value}" : wholePart;
        var isValid = decimal.TryParse(number, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var price)
            && price > 0
            && price <= Asset.MaxPurchasePrice;
        return (isValid, price);
    };

    /// <summary>A date in <see cref="Format.DateFormat"/> between <paramref name="earliest"/> and <paramref name="latest"/>, inclusive.</summary>
    internal static Func<string, (bool isValid, DateOnly result)> Date(DateOnly earliest, DateOnly latest) => input =>
    {
        var isValid = DateOnly.TryParseExact(input.Trim(), Format.DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            && date >= earliest
            && date <= latest;
        return (isValid, date);
    };

    // whole:    plain digits, or 1–3 digits then groups of exactly 3 with one consistent separator.
    // decimal:  "." or "," followed by 1–2 digits (prices have at most 2 decimals).
    [GeneratedRegex(@"^(?<whole>\d+|\d{1,3}(?<group>[ .,])\d{3}(?:\k<group>\d{3})*)(?:(?<decimal>[.,])(?<fraction>\d{1,2}))?$", RegexOptions.CultureInvariant)]
    private static partial Regex PricePattern();
}
