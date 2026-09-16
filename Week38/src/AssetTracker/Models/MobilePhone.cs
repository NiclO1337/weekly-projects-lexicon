namespace AssetTracker.Models;

internal sealed class MobilePhone(int id, string brand, string model, DateTime purchaseDate, decimal priceEur, Office office)
    : Asset(id, brand, model, purchaseDate, priceEur, office)
{
    internal override string GetCategoryLabel()
    {
        return "Phone";
    }
}
