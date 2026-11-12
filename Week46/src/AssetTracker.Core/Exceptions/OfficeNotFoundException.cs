namespace AssetTracker.Core.Exceptions;

/// <summary>Thrown when no office exists with the requested ID.</summary>
public sealed class OfficeNotFoundException : Exception
{
    /// <summary>The ID that could not be found.</summary>
    public int Id { get; }

    /// <summary>Creates a new <see cref="OfficeNotFoundException"/> for the given <paramref name="id"/>.</summary>
    public OfficeNotFoundException(int id)
        : base($"No office found with ID {id}.")
    {
        Id = id;
    }
}
