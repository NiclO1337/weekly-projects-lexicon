# Weekly mini projects

Weekly mini projects from the C# course at Lexicon, built by Niclas Hugdahl.
Each week has its own folder with a C# console application on .NET 10.

| Week | Project | Documentation |
|------|---------|---------------|
| [36](#week-36--product-list-manager) | Product list manager | – |
| [37](#week-37--product-management-system) | Product management system | [README](Week37/README.md) |
| [38](#week-38--asset-tracking-system) | Asset tracking system | [README](Week38/README.md) |
| [46](#week-46--asset-tracking-system-with-a-database) | Asset tracking system with a database (in progress) | [README](Week46/README.md) |

## Projects

### Week 36 – Product list manager

A product list manager built in four levels, each one adding to the last. All
four can be started from the same menu.

- **Focus:** C# basics, lists, loops, menus and input handling
- **Run:** `dotnet run --project Week36`

### Week 37 – Product management system

A product catalog with categories. Products can be added, edited, deleted,
sorted and searched, with statistics and data saved to a JSON file.

- **Focus:** classes, LINQ, input validation and saving to file
- **Run:** `dotnet run --project Week37`
- [README](Week37/README.md)

### Week 38 – Asset tracking system

Tracks a company's computers, phones and tablets across offices in four
countries. It converts prices to each office's currency with live exchange
rates, warns when an asset nears the end of its three-year lifetime, and
exports to CSV.

- **Focus:** inheritance, polymorphism, interfaces and exception handling
- **Run:** `dotnet run --project Week38/src/AssetTracker`
- [README](Week38/README.md)

### Week 46 – Asset tracking system with a database

The week 38 asset tracker rebuilt on a SQL Server database, split into domain,
persistence and user interface projects. It adds reports, search and filters,
and export to TXT, CSV and JSON. **In progress.**

- **Focus:** Entity Framework Core, migrations, LINQ queries and layered architecture
- **Built with:** Entity Framework Core, SQL Server Express and Spectre.Console
- [README](Week46/README.md) · [Design and build plan](Week46/docs/PROJECT_CONTEXT.md)

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Week 46 also needs SQL Server Express. See its [README](Week46/README.md#getting-started).
