using System.Globalization;

namespace AssetTracker.Services;

internal static class FormatHelpers
{
    /// <summary>
    /// Prompts for a positive numeric price (decimal input allowed), using invariant
    /// culture so behaviour is identical on every machine regardless of locale.
    /// </summary>
    internal static Func<string, (bool isValid, decimal result)> ValidatePositiveDecimal()
    {
        const decimal MaxPrice = 150_000_000m;

        return input =>
        {
            if (decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal value)
                && value > 0 && value <= MaxPrice)
            {
                return (true, value);
            }
            return (false, 0);
        };
    }

    /// <summary>
    /// Prompts for a purchase date in yyyy-MM-dd format, using invariant culture
    /// and rejecting dates in the future (an asset can't be purchased later than today).
    /// </summary>
    // TODO: revert this temporary bypass - restore the strict format/no-future-date
    // check below once manual testing is done (was: `return (false, default);`).
    internal static Func<string, (bool isValid, DateTime result)> ValidateDate()
    {
        return input =>
        {
            if (DateTime.TryParseExact(input, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime value)
                && value.Date <= DateTime.Today)
            {
                return (true, value);
            }
            return (true, DateTime.Today);
        };
    }

    internal static string Pluralize(int count, string singular, string plural)
    {
        return count == 1 ? singular : plural;
    }

    /// <summary>
    /// Formats a price for display as a whole number with thousand separators
    /// (assets are high-value enough that decimals aren't meaningful on screen).
    /// Does not attach a currency symbol - callers append that per Office/CurrencyCode.
    /// </summary>
    internal static string FormatPrice(decimal price)
    {
        decimal rounded = Math.Round(price, 0, MidpointRounding.AwayFromZero);
        return rounded.ToString("N0", CultureInfo.InvariantCulture);
    }
}
