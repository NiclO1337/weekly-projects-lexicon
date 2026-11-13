using AssetTracker.Core.Exceptions;
using AssetTracker.Core.Models;

namespace AssetTracker.Core.Abstractions;

/// <summary>
/// Loads offices. Offices are seeded by the database and never created or changed by the app.
/// Implementations throw <see cref="DataStoreException"/> when the store cannot be reached.
/// </summary>
public interface IOfficeRepository
{
    /// <summary>Every office, ordered by name.</summary>
    IReadOnlyList<Office> GetAll();

    /// <summary>The office with <paramref name="id"/>, or <see langword="null"/> if there is none.</summary>
    Office? FindById(int id);
}
