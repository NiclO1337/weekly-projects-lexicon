namespace AssetTracker.Core.Enums;

/// <summary>The broad kind of asset. Also the discriminator that decides which subclass a row loads as.</summary>
public enum AssetCategory
{
    /// <summary>A laptop or desktop computer.</summary>
    Computer,

    /// <summary>A phone or tablet.</summary>
    Mobile,
}
