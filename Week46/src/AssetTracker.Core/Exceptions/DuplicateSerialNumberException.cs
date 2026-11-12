namespace AssetTracker.Core.Exceptions;

/// <summary>Thrown when an asset would get a serial number that another asset already has.</summary>
public sealed class DuplicateSerialNumberException : Exception
{
    /// <summary>The serial number that is already in use.</summary>
    public string SerialNumber { get; }

    /// <summary>Creates a new <see cref="DuplicateSerialNumberException"/> for <paramref name="serialNumber"/>.</summary>
    public DuplicateSerialNumberException(string serialNumber)
        : base($"An asset with serial number '{serialNumber}' already exists.")
    {
        SerialNumber = serialNumber;
    }
}
