using AssetTracker.Core.Abstractions;
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
}
