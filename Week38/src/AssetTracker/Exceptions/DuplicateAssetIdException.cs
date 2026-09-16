namespace AssetTracker.Exceptions;

internal sealed class DuplicateAssetIdException(int id) : Exception($"An asset with Id {id} already exists.")
{
}
