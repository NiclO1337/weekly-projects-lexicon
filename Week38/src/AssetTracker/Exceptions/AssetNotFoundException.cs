namespace AssetTracker.Exceptions;

internal sealed class AssetNotFoundException(int id) : Exception($"No asset found with Id {id}.")
{
}
