using AssetTracker.Core.Enums;

namespace AssetTracker.Core.Models;

/// <summary>
/// An office that owns assets. The four offices are seeded by the database migration and are not
/// created or edited by the application, so there is no public constructor.
/// </summary>
public sealed class Office
{
    /// <summary>The maximum allowed length of <see cref="Name"/> and <see cref="Country"/>.</summary>
    public const int MaxNameLength = 50;

    /// <summary>Database identity.</summary>
    public int Id { get; private set; }

    /// <summary>Unique display name, e.g. "Sweden".</summary>
    public string Name { get; private set; }

    /// <summary>The country the office is in.</summary>
    public string Country { get; private set; }

    /// <summary>The currency assets created in this office are priced in.</summary>
    public CurrencyCode Currency { get; private set; }

#pragma warning disable CS8618 // EF Core sets every property when it loads the entity.
    private Office()
    {
    }
#pragma warning restore CS8618
}
