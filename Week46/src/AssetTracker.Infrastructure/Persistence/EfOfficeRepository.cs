using AssetTracker.Core.Abstractions;
using AssetTracker.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace AssetTracker.Infrastructure.Persistence;

/// <summary>Loads offices from SQL Server through EF Core.</summary>
public sealed class EfOfficeRepository(AppDbContext context) : IOfficeRepository
{
    public IReadOnlyList<Office> GetAll() => DatabaseGuard.Run(() =>
        context.Offices
            .AsNoTracking()
            .OrderBy(o => o.Name)
            .ToList());

    // Tracked on purpose: a new asset points at this office, and a tracked office is saved as
    // "already exists". An untracked one would be inserted again as a new row.
    public Office? FindById(int id) => DatabaseGuard.Run(() => context.Offices.Find(id));
}
