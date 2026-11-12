using AssetTracker.Core.Enums;

namespace AssetTracker.Core.Services;

/// <summary>
/// The 3-year lifetime rule, in one place. Expressed as purchase-date cutoffs: an asset bought
/// on or before a cutoff has that status. The status on an asset and the database filters
/// ("expired", "close to expiration") are computed from the same dates, so they can't disagree.
/// </summary>
public static class AssetLifecycle
{
    /// <summary>How many years an asset lasts from its purchase date.</summary>
    public const int LifespanYears = 3;

    /// <summary>Months of lifetime left at which an asset turns <see cref="EndOfLifeStatus.Yellow"/>.</summary>
    public const int YellowWarningMonths = 6;

    /// <summary>Months of lifetime left at which an asset turns <see cref="EndOfLifeStatus.Red"/>.</summary>
    public const int RedWarningMonths = 3;

    /// <summary>Assets bought on or before this date are past their lifetime (<see cref="EndOfLifeStatus.DarkRed"/>).</summary>
    public static DateOnly ExpiredCutoff(DateOnly today) => today.AddYears(-LifespanYears);

    /// <summary>Assets bought on or before this date have less than 3 months left (<see cref="EndOfLifeStatus.Red"/> or worse).</summary>
    public static DateOnly RedCutoff(DateOnly today) => ExpiredCutoff(today).AddMonths(RedWarningMonths);

    /// <summary>Assets bought on or before this date have less than 6 months left (<see cref="EndOfLifeStatus.Yellow"/> or worse).</summary>
    public static DateOnly YellowCutoff(DateOnly today) => ExpiredCutoff(today).AddMonths(YellowWarningMonths);

    /// <summary>The status of an asset bought on <paramref name="purchaseDate"/>, as of <paramref name="today"/>.</summary>
    public static EndOfLifeStatus GetStatus(DateOnly purchaseDate, DateOnly today) => purchaseDate switch
    {
        _ when purchaseDate <= ExpiredCutoff(today) => EndOfLifeStatus.DarkRed,
        _ when purchaseDate <= RedCutoff(today) => EndOfLifeStatus.Red,
        _ when purchaseDate <= YellowCutoff(today) => EndOfLifeStatus.Yellow,
        _ => EndOfLifeStatus.None,
    };
}
