using AssetTracker.Core.Enums;
using AssetTracker.Core.Models;

namespace AssetTracker.Infrastructure.Persistence;

/// <summary>
/// Week38's 25 sample assets, priced in each office's own currency. Purchase dates are given as
/// "months ago" so every lifetime status (normal, monitor, upgrade soon, end of life) is visible
/// whatever day the app is first run. Serial numbers, warranty dates and users are invented.
/// </summary>
internal static class SampleData
{
    private const int ComputerWarrantyYears = 3;
    private const int MobileWarrantyYears = 2;

    /// <param name="offices">The seeded offices, tracked by the context so they are not inserted again.</param>
    /// <param name="today">The date the purchase dates are counted back from.</param>
    internal static IReadOnlyList<Asset> Create(IReadOnlyList<Office> offices, DateOnly today)
    {
        var officesByName = offices.ToDictionary(o => o.Name);

        AssetDetails Details(string brand, string model, string serialNumber, int monthsAgo, int warrantyYears, decimal price, string officeName, string? assignedTo)
        {
            var purchaseDate = today.AddMonths(-monthsAgo);
            return new AssetDetails(brand, model, serialNumber, purchaseDate, purchaseDate.AddYears(warrantyYears), price, officesByName[officeName].Id, assignedTo);
        }

        Asset Computer(ComputerType type, string brand, string model, string serialNumber, int monthsAgo, decimal price, string officeName, string? assignedTo = null) =>
            new ComputerAsset(type, Details(brand, model, serialNumber, monthsAgo, ComputerWarrantyYears, price, officeName, assignedTo), officesByName[officeName]);

        Asset Mobile(MobileType type, string brand, string model, string serialNumber, int monthsAgo, decimal price, string officeName, string? assignedTo = null) =>
            new MobileAsset(type, Details(brand, model, serialNumber, monthsAgo, MobileWarrantyYears, price, officeName, assignedTo), officesByName[officeName]);

        return
        [
            Computer(ComputerType.Laptop, "Apple", "MacBook Pro", "C02APL1001", 14, 1_800m, "Germany", "jdoe"),
            Mobile(MobileType.Phone, "Samsung", "Galaxy S21", "R58SAM2001", 34, 650m, "Germany"),
            Computer(ComputerType.Desktop, "Dell", "OptiPlex 7090", "DL7090-3001", 39, 9_990m, "Sweden"),
            Mobile(MobileType.Tablet, "Apple", "iPad Air", "DMPAPL4001", 8, 7_990m, "Sweden", "asmith"),
            Computer(ComputerType.Laptop, "Lenovo", "ThinkPad X1", "PF-LNV-5001", 32, 1_549m, "USA", "bjones"),
            Mobile(MobileType.Phone, "Google", "Pixel 6", "GGL6-6001", 40, 599m, "USA"),
            Mobile(MobileType.Tablet, "Samsung", "Galaxy Tab S8", "R52SAM7001", 20, 29_999m, "Turkey"),
            Computer(ComputerType.Desktop, "HP", "EliteDesk 800", "MXL-HP-8001", 10, 38_999m, "Turkey", "ckaya"),
            Mobile(MobileType.Tablet, "Microsoft", "Surface Pro 8", "MSP8-9001", 35, 1_100m, "Germany"),
            Mobile(MobileType.Phone, "OnePlus", "9 Pro", "OP9P-10001", 31, 6_990m, "Sweden"),
            Computer(ComputerType.Laptop, "Asus", "ZenBook 14", "ASZB-11001", 5, 1_200m, "Germany", "mmuller"),
            Mobile(MobileType.Phone, "Apple", "iPhone 13", "F2LAPL12001", 12, 8_490m, "Sweden", "elarsson"),
            Mobile(MobileType.Tablet, "Samsung", "Galaxy Tab A8", "R9ASAM13001", 17, 229m, "USA"),
            Computer(ComputerType.Desktop, "Acer", "Aspire TC", "ACR-TC-14001", 26, 27_999m, "Turkey"),
            Mobile(MobileType.Phone, "Sony", "Xperia 5", "SNY-X5-15001", 32, 800m, "Germany"),
            Computer(ComputerType.Laptop, "Microsoft", "Surface Laptop 5", "MSL5-16001", 34, 17_990m, "Sweden", "pnilsson"),
            Mobile(MobileType.Tablet, "Lenovo", "Tab P11", "HA-LNV-17001", 31, 329m, "USA"),
            Mobile(MobileType.Phone, "Xiaomi", "Redmi Note 12", "XMI-RN12-18001", 34, 11_999m, "Turkey"),
            Computer(ComputerType.Desktop, "Dell", "Inspiron 3910", "DL3910-19001", 37, 650m, "Germany"),
            Mobile(MobileType.Tablet, "Apple", "iPad Mini", "DMQAPL20001", 38, 5_990m, "Sweden"),
            Mobile(MobileType.Phone, "Samsung", "Galaxy A53", "R5CSAM21001", 35, 449m, "USA", "tbrown"),
            Computer(ComputerType.Laptop, "HP", "Pavilion 15", "CND-HP-22001", 41, 34_999m, "Turkey"),
            Mobile(MobileType.Tablet, "Samsung", "Galaxy Tab S6", "R52SAM23001", 44, 505.50m, "Germany"),
            Mobile(MobileType.Phone, "Google", "Pixel 5", "GGL5-24001", 50, 5_490m, "Sweden"),
            Computer(ComputerType.Desktop, "Lenovo", "ThinkCentre M75", "TC-LNV-25001", 39, 649m, "USA"),
        ];
    }
}
