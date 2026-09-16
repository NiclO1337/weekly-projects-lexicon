namespace AssetTracker.Models;

internal sealed class Computer(int id, string brand, string model, DateTime purchaseDate, decimal priceEur, Office office, ComputerType computerType)
    : Asset(id, brand, model, purchaseDate, priceEur, office)
{
    public ComputerType ComputerType { get; set; } = computerType;

    internal override string GetCategoryLabel()
    {
        return ComputerType.ToString();
    }
}
