using AssetTracker.Core.Enums;

namespace AssetTracker.Core.Models;

/// <summary>A phone or tablet.</summary>
public sealed class MobileAsset : Asset
{
    /// <summary>The usual warranty length for a phone or tablet, offered as the default when one is added.</summary>
    public const int StandardWarrantyYears = 2;

    /// <summary>Phone or tablet. Fixed once the asset is created.</summary>
    public MobileType Type { get; private set; }

    /// <inheritdoc/>
    public override AssetCategory Category => AssetCategory.Mobile;

    /// <inheritdoc/>
    public override string TypeLabel => Type.ToString();

    /// <summary>Creates a validated mobile asset in <paramref name="office"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="type"/> is not a defined <see cref="MobileType"/>.</exception>
    /// <inheritdoc cref="Asset(AssetDetails, Office)"/>
    public MobileAsset(MobileType type, AssetDetails details, Office office)
        : base(details, office)
    {
        Type = Enum.IsDefined(type) ? type : throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown mobile type.");
    }

    private MobileAsset()
    {
    }
}
