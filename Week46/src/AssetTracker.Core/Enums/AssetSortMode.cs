namespace AssetTracker.Core.Enums;

/// <summary>The orders an asset list can be shown in.</summary>
public enum AssetSortMode
{
    /// <summary>By office name, then oldest purchase first. The default.</summary>
    Office,

    /// <summary>Computers before mobile devices, each grouped by type (desktops, laptops; phones, tablets), then oldest purchase first.</summary>
    Category,

    /// <summary>Oldest purchase first, which is the asset whose lifetime ends soonest.</summary>
    EndOfLife,
}
