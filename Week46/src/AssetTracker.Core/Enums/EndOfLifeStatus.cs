namespace AssetTracker.Core.Enums;

/// <summary>How close an asset is to the end of its lifetime. Decided by <see cref="Services.AssetLifecycle"/>.</summary>
public enum EndOfLifeStatus
{
    /// <summary>More than 6 months of lifetime left.</summary>
    None,

    /// <summary>Less than 6 months left.</summary>
    Yellow,

    /// <summary>Less than 3 months left.</summary>
    Red,

    /// <summary>Past the end of its lifetime.</summary>
    DarkRed,
}
