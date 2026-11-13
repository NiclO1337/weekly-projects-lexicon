namespace AssetTracker.Core.Enums;

/// <summary>The orders an asset list can be shown in.</summary>
public enum AssetSortMode
{
    /// <summary>Computers before mobile devices, then oldest purchase first. The default.</summary>
    Category,

    /// <summary>By office name, then oldest purchase first.</summary>
    Office,

    /// <summary>Oldest purchase first, which is the asset whose lifetime ends soonest.</summary>
    EndOfLife,
}
