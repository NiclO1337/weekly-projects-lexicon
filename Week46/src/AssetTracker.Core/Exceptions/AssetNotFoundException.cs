namespace AssetTracker.Core.Exceptions;

/// <summary>Thrown when no asset exists with the requested ID.</summary>
public sealed class AssetNotFoundException : Exception
{
    /// <summary>The ID that could not be found.</summary>
    public int Id { get; }

    /// <summary>Creates a new <see cref="AssetNotFoundException"/> for the given <paramref name="id"/>.</summary>
    public AssetNotFoundException(int id)
        : base($"No asset found with ID {id}.")
    {
        Id = id;
    }
}
