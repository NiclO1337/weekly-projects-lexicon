# Dragon's Hoard — Asset Tracking System

A console application for tracking company IT assets (computers, mobile phones, and tablets)
across multiple offices, built as a weekly mini-project in C#.

## Features

- **Add, view, search, edit, and remove assets** — computers (laptop/desktop), mobile phones, and tablets
- **Multi-office support** — Germany, Sweden, USA, and Turkey, each with its own currency
- **Live currency conversion** — fetches EUR-based exchange rates from the [Frankfurter API](https://frankfurter.dev/),
  with same-day caching and automatic fallback to cached or hardcoded rates when offline
- **End-of-life tracking** — flags assets approaching or past their 3-year lifespan
- **Sorting** — by office, asset type, or end-of-life status
- **CSV export**
- **Persistent storage** — assets are saved to a local JSON file between runs

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

## Running

```bash
dotnet run --project src/AssetTracker
```

## Data

Assets, exchange rate cache, and CSV exports are stored in `src/AssetTracker/Data/`.
