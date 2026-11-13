using AssetTracker.Core.Enums;

namespace AssetTracker.Core.Models;

/// <summary>A laptop or desktop computer.</summary>
public sealed class ComputerAsset : Asset
{
    /// <summary>The usual warranty length for a computer, offered as the default when one is added.</summary>
    public const int StandardWarrantyYears = 3;

    /// <summary>Laptop or desktop. Fixed once the asset is created.</summary>
    public ComputerType Type { get; private set; }

    /// <inheritdoc/>
    public override AssetCategory Category => AssetCategory.Computer;

    /// <inheritdoc/>
    public override string TypeLabel => Type.ToString();

    /// <summary>Creates a validated computer asset in <paramref name="office"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="type"/> is not a defined <see cref="ComputerType"/>.</exception>
    /// <inheritdoc cref="Asset(AssetDetails, Office)"/>
    public ComputerAsset(ComputerType type, AssetDetails details, Office office)
        : base(details, office)
    {
        Type = Enum.IsDefined(type) ? type : throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown computer type.");
    }

    private ComputerAsset()
    {
    }
}
