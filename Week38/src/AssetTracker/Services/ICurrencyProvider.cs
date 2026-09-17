using AssetTracker.Models;

namespace AssetTracker.Services;

internal interface ICurrencyProvider
{
    decimal GetRate(CurrencyCode currency);
}
