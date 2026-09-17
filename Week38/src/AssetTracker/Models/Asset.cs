using System.Text.Json.Serialization;

namespace AssetTracker.Models;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "assetType")]
[JsonDerivedType(typeof(Computer), "Computer")]
[JsonDerivedType(typeof(MobilePhone), "MobilePhone")]
[JsonDerivedType(typeof(Tablet), "Tablet")]
internal abstract class Asset(int id, string brand, string model, DateTime purchaseDate, decimal priceEur, Office office)
{
    internal const int LifespanYears = 3;

    public int Id { get; set; } = id;
    public string Brand { get; set; } = brand;
    public string Model { get; set; } = model;

    [JsonConverter(typeof(DateOnlyJsonConverter))]
    public DateTime PurchaseDate { get; set; } = purchaseDate;

    public decimal PriceEur { get; set; } = priceEur;
    public Office Office { get; set; } = office;

    internal int GetAgeInDays()
    {
        return (DateTime.Today - PurchaseDate).Days;
    }

    internal DateTime GetEndOfLifeDate()
    {
        return PurchaseDate.AddYears(LifespanYears);
    }

    internal EndOfLifeStatus GetEndOfLifeStatus()
    {
        DateTime endOfLifeDate = GetEndOfLifeDate();

        if (DateTime.Today >= endOfLifeDate)
        {
            return EndOfLifeStatus.DarkRed;
        }

        if (DateTime.Today >= endOfLifeDate.AddMonths(-3))
        {
            return EndOfLifeStatus.Red;
        }

        if (DateTime.Today >= endOfLifeDate.AddMonths(-6))
        {
            return EndOfLifeStatus.Yellow;
        }

        return EndOfLifeStatus.None;
    }

    internal string GetEndOfLifeStatusLabel()
    {
        return GetEndOfLifeStatus() switch
        {
            EndOfLifeStatus.Yellow => "Monitor",
            EndOfLifeStatus.Red => "Upgrade soon",
            EndOfLifeStatus.DarkRed => "End of life",
            _ => "",
        };
    }

    internal abstract string GetCategoryLabel();
}
