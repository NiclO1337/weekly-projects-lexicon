namespace AssetTracker.Core.Exceptions;

/// <summary>
/// Thrown when the database could not be reached, read or written. Wraps the original exception
/// (typically a <c>SqlException</c> or <c>DbUpdateException</c>) so callers outside the data
/// layer never depend on database types.
/// </summary>
public sealed class DataStoreException : Exception
{
    /// <summary>Creates a new <see cref="DataStoreException"/> wrapping <paramref name="innerException"/>.</summary>
    public DataStoreException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
