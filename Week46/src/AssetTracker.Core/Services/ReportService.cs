using AssetTracker.Core.Abstractions;
using AssetTracker.Core.Enums;
using AssetTracker.Core.Exceptions;
using AssetTracker.Core.Models;

namespace AssetTracker.Core.Services;

/// <summary>
/// Builds the reports. Anything totalled across offices is converted to euros first: the database
/// sums per currency, and only those few sums are converted here.
/// </summary>
/// <param name="assets">Where assets are stored.</param>
/// <param name="offices">Where offices are loaded from.</param>
/// <param name="currency">Exchange rates against the euro.</param>
public sealed class ReportService(IAssetRepository assets, IOfficeRepository offices, ICurrencyProvider currency)
{
    /// <summary>How many assets <see cref="MostExpensive"/> lists unless told otherwise.</summary>
    public const int DefaultTopCount = 10;

    // The columns that identify an asset, shared by every report that lists assets.
    private static readonly ReportColumn[] s_assetColumns =
        [new("Id", IsNumeric: true), new("Type"), new("Brand"), new("Model"), new("Office")];

    /// <summary>
    /// Assets with less than 6 months of lifetime left that haven't reached the end yet (the
    /// "Monitor" and "Upgrade soon" statuses), the soonest to expire first.
    /// </summary>
    /// <exception cref="DataStoreException">The store could not be read.</exception>
    public Report CloseToExpiration()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        // Same cutoffs as the status labels, so the report and the colours can't disagree.
        var expiring = assets.GetAll(
            new AssetQuery { SortBy = AssetSortMode.EndOfLife },
            purchasedAfter: AssetLifecycle.ExpiredCutoff(today),
            purchasedOnOrBefore: AssetLifecycle.YellowCutoff(today));

        return new Report(
            "Assets close to expiration",
            [.. s_assetColumns, new("Purchased"), new("Lifetime ends"), new("Status")],
            [.. expiring.Select(a => (IReadOnlyList<object>)
                [.. AssetCells(a), a.PurchaseDate, AssetLifecycle.EndOfLifeDate(a.PurchaseDate), a.GetEndOfLifeStatusLabel(today)])]);
    }

    /// <summary>
    /// The <paramref name="count"/> most valuable assets across all offices, ranked by their value
    /// in EUR, so prices in different currencies are compared fairly.
    /// </summary>
    /// <exception cref="DataStoreException">The store could not be read.</exception>
    public Report MostExpensive(int count = DefaultTopCount)
    {
        // The overall top n is always among the top n of each currency, so ranking those few
        // candidates after conversion gives the exact answer.
        var ranked = assets.GetMostExpensivePerCurrency(count)
            .Select(a => (Asset: a, ValueEur: currency.ToEur(a.PurchasePrice, a.Currency)))
            .OrderByDescending(x => x.ValueEur)
            .ThenBy(x => x.Asset.Id)
            .Take(count)
            .ToList();

        return new Report(
            $"Most expensive assets (top {count})",
            [new("Rank", IsNumeric: true), .. s_assetColumns, new("Price", IsNumeric: true), new("Code"), new("Value (EUR)", IsNumeric: true)],
            [.. ranked.Select((x, index) => (IReadOnlyList<object>)
                [index + 1, .. AssetCells(x.Asset), x.Asset.PurchasePrice, x.Asset.Currency.ToString(), x.ValueEur])]);
    }
    /// <summary>
    /// Asset count and total value in EUR for every office, including offices with no assets, and
    /// a final "All offices" row.
    /// </summary>
    /// <exception cref="DataStoreException">The store could not be read.</exception>
    public Report OfficeSummary()
    {
        var totals = assets.GetOfficeTotals();

        var perOffice = offices.GetAll()
            .Select(office =>
            {
                var officeTotals = totals.Where(t => t.OfficeId == office.Id).ToList();
                return (
                    Name: office.Name,
                    Count: officeTotals.Sum(t => t.AssetCount),
                    ValueEur: officeTotals.Sum(t => currency.ToEur(t.TotalPrice, t.Currency)));
            })
            .ToList();

        IReadOnlyList<object> allOffices = ["All offices", perOffice.Sum(o => o.Count), perOffice.Sum(o => o.ValueEur)];

        return new Report(
            "Value and count per office",
            [new ReportColumn("Office"), new ReportColumn("Assets", IsNumeric: true), new ReportColumn("Value (EUR)", IsNumeric: true)],
            [.. perOffice.Select(o => (IReadOnlyList<object>)[o.Name, o.Count, o.ValueEur]), allOffices]);
    }

    private static object[] AssetCells(Asset asset) => [asset.Id, asset.TypeLabel, asset.Brand, asset.Model, asset.Office.Name];
}
