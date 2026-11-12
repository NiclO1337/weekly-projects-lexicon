# AssetTracker (Week 46) — Project Context

Reference document for the design and the build plan. Read this before writing code;
`CLAUDE.md` is the short rulebook derived from it.

- **Course project:** Weekly Mini Project week 46 — Smart Asset Tracking System
- **Platform:** .NET 10 (LTS), C# 14
- **Persistence:** Entity Framework Core 10, SQL Server Express, code-first migrations
- **UI:** Spectre.Console for output, numbered menus for input
- **Currency:** prices are stored in the local currency they were paid in; EUR is the common
  currency for values, reports and totals
- **Target:** brief levels 1–4. Level 5 only if there is time (see §14)

This is Week38's AssetTracker rebuilt on a database, with the UI upgrades and the three-project
split from the Milestone project (MoneyTracker). Reuse comes first: §11 maps every existing
file to where it lands here.

---

## 1. Requirements from the brief

### General

- [ ] Each asset has: Id, asset type, brand, model name, purchase date, purchase price, local
      price (converted), office location, serial number, employee/username (optional),
      warranty expiration date
- [ ] Asset lifetime is 3 years, with coloured warnings near the end
- [ ] Entity Framework Core, SQL Server, migrations, LINQ queries
- [ ] Repository / service structure
- [ ] Exception handling and validation of incorrect input

### Level 1 — Basic asset registration

- [ ] Classes for computer assets (laptop, desktop) and mobile assets (phones, tablet)
- [ ] Base class `Asset` with `ComputerAsset` and `MobileAsset`, proper constructors and properties
- [ ] Create assets and save them to the database
- [ ] Read and display all assets

### Level 2 — Full CRUD and asset status

- [ ] Create, read, update, delete
- [ ] Sort by category first, then by purchase date
- [ ] Highlight assets by remaining lifetime
- [ ] Menu-driven navigation, reusable methods, LINQ for sorting and filtering
- [ ] Validation for invalid dates and empty fields

### Level 3 — Global office management

- [ ] Offices (Sweden, USA, Germany, Turkey) with name, country, currency
- [ ] Convert prices using exchange rates (fixed and live API) — here from local currency to
      EUR, see §2
- [ ] Report: total asset value per office
- [ ] Report: asset count per office
- [ ] Report: assets close to expiration
- [ ] Report: most expensive assets

### Level 4 — Search, filtering, export

- [ ] Search by brand, model, office, purchase year
- [ ] Filter: expired only, computers only, mobile devices only, by office
- [ ] Export to TXT, CSV and JSON
- [ ] Pagination for large lists (optional suggestion — included)
- [ ] Async database operations, logging, configuration files (optional suggestions — not
      planned, see §14)

### The brief's "important notes"

Meaningful names, no duplicated code, small reusable methods, comments where necessary, clean
code, careful testing, graceful handling of invalid input. The definition of done in §13 checks
these.

---

## 2. Agreed decisions

Everything below is agreed with Niclas (last reviewed 2026-11-12). A new or changed decision
goes in as "Proposed" until he confirms it.

| Topic | Decision | Status |
|---|---|---|
| Scope | Levels 1–4. Level 5 only if there is time | Agreed |
| Architecture | `Core` / `Infrastructure` / `ConsoleApp`, as in the Milestone project | Agreed |
| Database | SQL Server Express via EF Core, same package versions and connection pattern as the `EntityFrameworkCore` example project | Agreed |
| Price currency | Prices are **entered and stored in local currency** — what the office actually paid. Each asset's value in EUR is calculated from today's rate, and reports and totals are in EUR. This reverses the brief (one entry currency, converted to local); the reasoning is below the table | Agreed |
| Currency ownership | The currency belongs to the **asset**, not the office. It is set from the office's currency when the asset is created and stays the same if the asset later moves to another office | Agreed |
| Cross-currency ranking | Prices in different currencies are never compared directly. "Most expensive" takes the top 10 per currency in the database, and the service converts and merges them. Office totals are summed per office and currency in SQL and converted in the service | Agreed |
| Common currency | EUR rather than the brief's USD; the teacher allows it | Agreed |
| Lifetime colours | Week38's three: Yellow → `DarkYellow`, Red → `Red`, DarkRed → `DarkRed`. The brief's wording is a typo | Agreed |
| Console library | **Spectre.Console from the start** | Agreed |
| Menus | Numbered, `0` = back/cancel/quit, `disabledChoices` for pagination | Agreed |
| Screen handling | Clear screen per screen (Milestone), no `OutputTracker` | Agreed |
| Intro | Week38's dragon art and text, paced by the Milestone `SlowConsole` | Agreed |
| Spectre's role | Spectre draws (tables, headings, panels, colour). Input stays `ConsoleInput`'s numbered menus — no Spectre prompts, since arrow-key selection would replace the `0`/`disabledChoices` menus | Agreed |
| Class hierarchy | `Asset` → `ComputerAsset` (Laptop/Desktop), `MobileAsset` (Phone/Tablet). Brands are data, not classes | Agreed |
| Changing type | Category and type are fixed after creation; to correct one, remove and re-add. (Week38's "Change asset type" swapped the object; with database identity IDs that would be a delete + insert) | Agreed |
| Dates | `DateOnly` in the model, `date` columns in SQL Server | Agreed |
| Database name | `asset_tracker_ef` on `localhost\SQLEXPRESS` | Agreed |
| Offices | A table seeded with the four offices. No UI for managing offices | Agreed |
| Search and filter | Built into the asset list screen (one screen, criteria combine), not a separate "Search" menu entry | Agreed |
| Table columns | List shows ID, type, brand, model, serial number, purchase date, office, price, currency code, value in EUR and lifetime status; warranty and assigned user show in the details view and in exports | Agreed |
| Sample data | Week38's 25 assets, re-dated relative to today and re-priced in local currency, inserted when the table is empty | Agreed |
| Tests | Manual only, no test project | Agreed |
| Project names | `AssetTracker.Core`, `.Infrastructure`, `.ConsoleApp`; solution `Week46.slnx` | Agreed |

**Why local prices.** An office buys its equipment locally: shipping a phone from Germany to
Turkey or the USA would cost more than buying it there. So the Swedish office pays 14 990 kr for
a laptop, and 14 990 kr is what its bookkeeping says. If the application stored a EUR price and
converted it to SEK for display, that laptop would show a different SEK price every time the
exchange rate moved — a price nobody paid. Storing what was paid keeps the fact fixed and lets
the derived number, the EUR value used to compare and total across offices, follow the rate
instead. The exchange-rate API, the fallback chain and the conversion logic the brief asks for
are all still used; only the direction changes.

---

## 3. Solution layout

```
Week46/
├── CLAUDE.md
├── Week46.slnx
├── Directory.Build.props                 # copied from the Milestone project
├── README.md
├── docs/
│   └── PROJECT_CONTEXT.md                # this file
└── src/
    ├── AssetTracker.Core/                # classlib — domain + business logic, no packages
    │   ├── Models/
    │   │   ├── Asset.cs
    │   │   ├── ComputerAsset.cs
    │   │   ├── MobileAsset.cs
    │   │   ├── Office.cs
    │   │   ├── AssetDetails.cs           # input record for add/update
    │   │   ├── AssetQuery.cs             # search + filter + sort + page
    │   │   ├── PagedResult.cs
    │   │   ├── OfficeCurrencyTotal.cs    # count + sum per office and currency, from SQL
    │   │   ├── OfficeSummary.cs          # per office, in EUR
    │   │   └── Report.cs                 # title + columns + rows, for screen and export
    │   ├── Enums/
    │   │   ├── AssetCategory.cs          # Computer, Mobile
    │   │   ├── ComputerType.cs           # Laptop, Desktop
    │   │   ├── MobileType.cs             # Phone, Tablet
    │   │   ├── CurrencyCode.cs           # EUR, SEK, USD, TRY
    │   │   ├── EndOfLifeStatus.cs        # None, Yellow, Red, DarkRed
    │   │   ├── LifecycleFilter.cs        # All, ExpiringSoon, Expired
    │   │   ├── AssetSortMode.cs          # Category, Office, EndOfLife
    │   │   └── ExportFormat.cs           # Txt, Csv, Json
    │   ├── Abstractions/
    │   │   ├── IAssetRepository.cs
    │   │   ├── IOfficeRepository.cs
    │   │   ├── ICurrencyProvider.cs
    │   │   └── IReportExporter.cs
    │   ├── Services/
    │   │   ├── AssetLifecycle.cs         # the 3-year rule and its cutoffs, in one place
    │   │   ├── AssetService.cs
    │   │   ├── ReportService.cs
    │   │   └── ExportService.cs
    │   └── Exceptions/
    │       ├── AssetNotFoundException.cs
    │       ├── OfficeNotFoundException.cs
    │       ├── DuplicateSerialNumberException.cs
    │       └── DataStoreException.cs
    ├── AssetTracker.Infrastructure/      # classlib — database, HTTP, files
    │   ├── Persistence/
    │   │   ├── AppDbContext.cs
    │   │   ├── DatabaseInitializer.cs    # Migrate() + sample data
    │   │   ├── SampleData.cs
    │   │   ├── EfAssetRepository.cs
    │   │   └── EfOfficeRepository.cs
    │   ├── Migrations/                   # generated
    │   ├── Currency/
    │   │   ├── ApiCurrencyProvider.cs
    │   │   ├── HardcodedCurrencyProvider.cs
    │   │   └── CurrencyRateSource.cs
    │   ├── Export/
    │   │   ├── TxtReportExporter.cs
    │   │   ├── CsvReportExporter.cs
    │   │   └── JsonReportExporter.cs
    │   ├── AppPaths.cs
    │   └── Data/                         # runtime files, gitignored
    │       ├── exchangeRates.json
    │       └── exports/
    └── AssetTracker.ConsoleApp/          # console — UI + composition root
        ├── Program.cs                    # intro, wiring, outro
        └── UI/
            ├── MainMenu.cs
            ├── AssetBrowser.cs           # the paginated list: sort, search, filter, pick by ID
            ├── AssetMenu.cs              # add / edit / remove flows
            ├── ReportMenu.cs             # reports + export flow
            ├── AssetTable.cs             # Spectre table for assets
            ├── AssetDetailsPanel.cs      # one asset, every field
            ├── ReportTable.cs            # Spectre table for any Report
            ├── ConsoleInput.cs
            ├── ConsoleMessage.cs
            ├── Theme.cs                  # every colour, once
            ├── InputValidators.cs
            ├── Format.cs
            ├── SlowConsole.cs
            └── UserCancelledException.cs
```

The current `Week46.csproj` / `Program.cs` ("Hello, World!") / `Week46.slnx` at the top of
`Week46/` are placeholders from Visual Studio; build step 1 replaces them.

### Project references

```
ConsoleApp ──► Core ◄── Infrastructure
     └────────────────────►┘
```

`Core` references nothing — no project, no NuGet package. It defines the interfaces;
`Infrastructure` implements them. The compiler enforces the direction: `Core` cannot name an EF
Core or Spectre type because it has no reference to either package. What the compiler does not
enforce (`Console`, `File`, `HttpClient` are visible everywhere) stays a `CLAUDE.md` rule.

### `Directory.Build.props`

Copied unchanged from the Milestone project: `net10.0`, `LangVersion latest`, nullable and
implicit usings on, `TreatWarningsAsErrors`, `EnforceCodeStyleInBuild`,
`InvariantGlobalization false`.

### Packages

| Package | Version | Project |
|---|---|---|
| `Microsoft.EntityFrameworkCore` | 10.0.14 | Infrastructure |
| `Microsoft.EntityFrameworkCore.SqlServer` | 10.0.14 | Infrastructure |
| `Microsoft.Extensions.Configuration.EnvironmentVariables` | 11.0.0 | Infrastructure |
| `Microsoft.Extensions.Configuration.UserSecrets` | 11.0.0 | Infrastructure |
| `System.Security.Cryptography.Pkcs` | 10.0.14 | Infrastructure |
| `Microsoft.EntityFrameworkCore.Tools` | 10.0.14 | ConsoleApp |
| `Spectre.Console` | 0.57.2 | ConsoleApp |

The first six are the versions in `EntityFrameworkCore/ConsoleAppEF1.csproj`. Spectre.Console
0.57.2 is the current stable release on NuGet (checked 2026-11-12) and targets `net10.0`.

---

## 4. Domain model

### `Asset` (abstract)

| Member | Type | Notes |
|---|---|---|
| `Id` | `int` | Database identity. Never set in code |
| `Brand` | `string` | Trimmed, non-empty, max 50 |
| `Model` | `string` | Trimmed, non-empty, max 50 |
| `SerialNumber` | `string` | Trimmed, non-empty, max 50, **unique** |
| `PurchaseDate` | `DateOnly` | Not in the future |
| `WarrantyExpirationDate` | `DateOnly` | Not before `PurchaseDate` |
| `PurchasePrice` | `decimal` | What was paid, in `Currency`. > 0 and ≤ 150 000 000 (Week38's limit), 2 decimals |
| `Currency` | `CurrencyCode` | Set from the office's currency at creation. Not changed by `Update`, even when the office changes |
| `OfficeId` / `Office` | `int` / `Office` | Required |
| `AssignedTo` | `string?` | Optional employee/username, max 50 |
| `Category` | `AssetCategory` | **abstract** — Computer / Mobile |
| `TypeLabel` | `string` | **abstract** — "Laptop", "Desktop", "Phone", "Tablet" (the "Type" column) |
| `GetEndOfLifeStatus(today)` | `EndOfLifeStatus` | Delegates to `AssetLifecycle` |
| `GetEndOfLifeStatusLabel(today)` | `string` | "", "Monitor", "Upgrade soon", "End of life" |
| `Update(AssetDetails)` | `void` | Re-validates and replaces the editable fields |

`ComputerAsset` adds `ComputerType Type`; `MobileAsset` adds `MobileType Type`. Both are
`sealed`. `Category` and `TypeLabel` are the polymorphic hooks: no screen, service or exporter
ever asks "is this a computer?".

Max lengths and the price limit are public constants on `Asset` (`MaxBrandLength`, …). The
domain validates against them and the Fluent API configuration uses the same constants, so the
column sizes and the validation can't drift apart.

Each entity has a private parameterless constructor for EF Core and a public constructor that
validates. Setters are private; `Update` is the only way to change an asset.

### `Office`

`Id`, `Name` (unique), `Country`, `Currency` (`CurrencyCode` — the currency that assets created
in this office are priced in). Seeded: Germany (EUR), Sweden
(SEK), USA (USD), Turkey (TRY). Week38's static `Office.Germany` etc. become rows.

### `AssetLifecycle` — the 3-year rule in one place

```csharp
public static class AssetLifecycle
{
    public const int LifespanYears = 3;

    public static DateOnly ExpiredCutoff(DateOnly today);   // today − 3 years
    public static DateOnly RedCutoff(DateOnly today);       // today − 3 years + 3 months
    public static DateOnly YellowCutoff(DateOnly today);    // today − 3 years + 6 months
    public static EndOfLifeStatus GetStatus(DateOnly purchaseDate, DateOnly today);
}
```

An asset bought on or before a cutoff has that status. Expressing the rule as purchase-date
cutoffs means the label on a row and the database filters ("expired only", "close to
expiration") are computed from the same three dates and can never disagree.

| Purchase date | Status | Label | Colour |
|---|---|---|---|
| after `YellowCutoff` | `None` | (blank) | none |
| ≤ `YellowCutoff` (less than 6 months left) | `Yellow` | Monitor | `ConsoleColor.DarkYellow` |
| ≤ `RedCutoff` (less than 3 months left) | `Red` | Upgrade soon | `ConsoleColor.Red` |
| ≤ `ExpiredCutoff` (past 3 years) | `DarkRed` | End of life | `ConsoleColor.DarkRed` |

### Query and result types

- `AssetDetails` — record with the editable fields (brand, model, serial number, purchase date,
  warranty date, price, office ID, assigned to). What the UI hands to the service. It carries
  no currency: the service takes that from the office when the asset is created.
- `AssetQuery` — record: `Brand?`, `Model?`, `OfficeId?`, `PurchaseYear?`, `Category?`,
  `Lifecycle` (`LifecycleFilter`), `SortBy` (`AssetSortMode`), `Page`, `PageSize`. Criteria
  combine with AND. Text criteria are case-insensitive "contains".
- `PagedResult<T>` — `Items`, `PageNumber`, `TotalPages`, `TotalCount` (replaces Week38's `AssetsPage`).
- `OfficeCurrencyTotal` — `Office`, `Currency`, `AssetCount`, `TotalPrice`. One row per office
  and currency, straight from a SQL `GROUP BY`.
- `OfficeSummary` — `Office`, `AssetCount`, `TotalEur`. Built by `ReportService` from the rows above.
- `Report` — `Title`, `Columns`, `Rows` (cell values, not pre-formatted strings). One shape for
  every report and for the asset list, so one screen renderer and three exporters cover all of them.

---

## 5. Services

All business logic lives here. Every read returns `IReadOnlyList<T>` or a result record.

```csharp
public sealed class AssetService(IAssetRepository assets, IOfficeRepository offices, ICurrencyProvider currency)
{
    public const int PageSize = 10;

    public PagedResult<Asset> GetPage(AssetQuery query);
    public Asset GetById(int id);                                   // throws AssetNotFoundException
    public IReadOnlyList<Office> GetOffices();
    public ComputerAsset AddComputer(ComputerType type, AssetDetails details);
    public MobileAsset AddMobile(MobileType type, AssetDetails details);
    public Asset Update(int id, AssetDetails details);
    public void Remove(int id);
    public decimal GetValueInEur(Asset asset);                      // PurchasePrice ÷ rate for the asset's currency
}

public sealed class ReportService(IAssetRepository assets, ICurrencyProvider currency)
{
    public Report OfficeSummary();                  // count + total value in EUR, per office
    public Report CloseToExpiration();              // Yellow and Red, soonest first
    public Report MostExpensive(int count = 10);    // ranked by value in EUR
    public Report AssetList(AssetQuery query);      // the current list, unpaged, for export
}

public sealed class ExportService(IEnumerable<IReportExporter> exporters)
{
    public bool Exists(string fileName, ExportFormat format);
    public string Export(Report report, string fileName, ExportFormat format);   // returns the full path
}
```

`AddComputer` / `AddMobile` follow the Milestone's `AddIncome` / `AddExpense`: the UI never
constructs an entity and never switches on type.

`AssetService` checks serial-number uniqueness and that the office exists before saving, and
throws the domain exception. On add it sets the asset's `Currency` from the chosen office. `GetPage` translates `LifecycleFilter` into purchase-date bounds
with `AssetLifecycle`, so the repository only ever sees plain dates.

---

## 6. Persistence (EF Core)

### Contracts (in `Core`)

```csharp
public interface IAssetRepository
{
    PagedResult<Asset> GetPage(AssetQuery query, DateOnly? purchasedAfter, DateOnly? purchasedOnOrBefore);
    IReadOnlyList<Asset> GetAll(AssetQuery query, DateOnly? purchasedAfter, DateOnly? purchasedOnOrBefore);
    Asset? FindById(int id);
    bool SerialNumberExists(string serialNumber, int? exceptAssetId = null);
    IReadOnlyList<OfficeCurrencyTotal> GetOfficeTotals();           // GROUP BY office and currency
    IReadOnlyList<Asset> GetMostExpensivePerCurrency(int count);    // top N within each currency
    void Add(Asset asset);
    void Update(Asset asset);
    void Remove(Asset asset);
}

public interface IOfficeRepository
{
    IReadOnlyList<Office> GetAll();
    Office? FindById(int id);
}
```

No `IQueryable`, no `DbSet`, no `Include` in these signatures. Assets always come back with
their `Office` loaded.

### `AppDbContext`

Same shape as the example project: a `ConfigurationBuilder` with user secrets and environment
variables, `APP_DB_CONNECTION` if set, otherwise the fallback

```
Server=localhost\SQLEXPRESS;Database=asset_tracker_ef;Integrated Security=true;Encrypt=false;
```

Differences from the example: **no `LogTo(Console.WriteLine, …)`**, and
`AssetTracker.Infrastructure.csproj` carries its own `<UserSecretsId>` because
`AddUserSecrets<AppDbContext>()` looks it up on the assembly that contains the context. The
parameterless constructor + `OnConfiguring` is what lets `dotnet ef` / Package Manager Console
create the context at design time.

### Model configuration (`OnModelCreating`)

- `Asset`: table-per-hierarchy, discriminator column `AssetCategory` with values `Computer` /
  `Mobile`. `HasMaxLength` from the `Asset` constants. Unique index on `SerialNumber`.
  `PurchasePrice` → `HasPrecision(18, 2)`. Office FK required, `DeleteBehavior.Restrict`.
- `ComputerType`, `MobileType`, `CurrencyCode` stored as strings (`HasConversion<string>()`), so
  the tables are readable in SSMS.
- `Office`: `Name` max 50 and unique; the four offices seeded with `HasData` (IDs 1–4).

### Queries

Built with LINQ and executed in SQL Server — the brief grades LINQ and EF.

| Need | How |
|---|---|
| Sort: Category | Discriminator, then `PurchaseDate` |
| Sort: Office | `Office.Name`, then `PurchaseDate` |
| Sort: End of life | `PurchaseDate` ascending (oldest expires first) |
| Brand / model search | `Contains` (SQL `LIKE`, case-insensitive under the default collation) |
| Purchase year | `PurchaseDate.Year == year` |
| Computers / mobile only | `OfType<ComputerAsset>()` / `OfType<MobileAsset>()` |
| Expired / expiring soon | `PurchaseDate` compared with the bounds from `AssetLifecycle` |
| Paging | `Skip` / `Take` after ordering, plus a `Count` for the page total |
| Value and count per office | `GroupBy` office and currency with `Count` and `Sum(PurchasePrice)`; the service converts each sum to EUR and adds them up per office |
| Most expensive | Top n by `PurchasePrice` within each currency (at most 4 × n rows); the service converts to EUR, sorts and keeps n |

Prices in different currencies are never compared or summed directly — 40 000 TRY is less than
1 500 EUR. Anything that ranks or totals across currencies follows the two patterns above:
aggregate per currency in SQL, then convert the few resulting rows in the service. The overall
top n is always among the per-currency top n, so the result is exact.

Lists use `AsNoTracking()`. `FindById` returns a tracked entity, so edit is load → `Update` →
`SaveChanges`.

### Lifetime of the context

One `AppDbContext` created in `Program.cs`, shared by both repositories, disposed at exit. No DI
container.

### Startup and sample data

`DatabaseInitializer.Initialize()` calls `Database.Migrate()` (creates the database on first
run, applies anything pending), then inserts `SampleData` if `Assets` is empty. `SampleData` is
Week38's `assets.json` (25 assets) with invented serial numbers and warranty dates, prices
restated as realistic local amounts (14 990 SEK, not a converted figure), and purchase
dates expressed as "months ago" so that normal, yellow, red and expired assets always exist
whatever day the teacher runs it.

### Failures

`SqlException` and `DbUpdateException` are caught inside `Infrastructure` and rethrown as
`DataStoreException`. At startup `Program.cs` prints a clear message (is SQL Server Express
running? is `APP_DB_CONNECTION` right?) and exits. Mid-session, `ConsoleInput.TryRun` shows the
error and returns to the menu.

---

## 7. Currency conversion

The providers are copied from Week38 with only the namespace changed: rates are still EUR-based
(how many units of a currency one euro buys). What changes is the direction. Week38 stored EUR
and multiplied to get the local price; here the local price is stored and the value in EUR is
`PurchasePrice ÷ rate`.

- `ICurrencyProvider.GetRate(CurrencyCode)` in `Core`; `AssetService.GetValueInEur` is the one
  place an asset's price is divided, rounded to 2 decimals. `ReportService` converts its
  per-currency totals the same way.
- EUR values are "at today's rate": they move with the exchange rate. The stored price never does.
- `ApiCurrencyProvider` calls Frankfurter
  (`https://api.frankfurter.dev/v1/latest?base=EUR&symbols=SEK,USD,TRY`), at most once per
  calendar day, caching in `Data/exchangeRates.json`. Order: today's cache → live fetch → stale
  cache → `HardcodedCurrencyProvider`. It never throws.
- It exposes `Source` (`TodayCache` / `LiveApi` / `StaleCache` / `Hardcoded`) and `CachedDate`;
  `Program.cs` reads them once at startup and reports success or a warning.
- This covers both options in the brief: fixed rates and a real exchange-rate API.

---

## 8. Reports and export

### Reports (Level 3)

| Report | Columns |
|---|---|
| Value and count per office | Office, Country, Assets, Total value (EUR) — plus a grand-total row. Valued at today's rates |
| Assets close to expiration | The asset columns, for Yellow and Red assets, soonest first |
| Most expensive assets | The asset columns, top 10 by value in EUR |

`ReportService` builds each as a `Report`. The UI renders it with `ReportTable`; the same object
goes to the exporters.

### Export (Level 4)

```csharp
public interface IReportExporter
{
    ExportFormat Format { get; }
    bool Exists(string fileName);
    string Export(Report report, string fileName);    // returns the full path written
}
```

| Format | Output |
|---|---|
| TXT | Title, generated date, then a plain aligned table |
| CSV | Header row + rows, invariant culture, fields escaped (Week38's `EscapeCsvField`) |
| JSON | `{ "title", "generatedOn", "rows": [ { column: value, … } ] }`, indented |

Available from every report screen and from the asset list (which exports the whole filtered and
sorted list, not just the visible page). Flow: pick format (`0` cancels) → file name without
extension (`q` cancels, invalid file-name characters re-prompt) → confirm if the file exists →
success message with the path. Files go to `Data/exports/` in `Infrastructure`, resolved by
`AppPaths` walking up from the build output, as Week38 and the Milestone did.

Exported asset rows contain every field, including serial number, warranty date, assigned
user, and both the price with its currency and the value in EUR. Numbers and dates are written in invariant format (`1500.00`, `2026-03-17`).

---

## 9. Console UI

### Startup (`Program.cs`)

```
SlowConsole.Install()
→ dragon art + welcome text (Week38's wording, typewriter effect)
→ "Connecting to database..."   → success: "Loaded 25 assets."   | DataStoreException: error, exit
→ "Fetching exchange rates..."  → success or warning, by Source
→ Pause
→ MainMenu.Run()
→ farewell text + small dragon (Week38)
```

`Program.cs` wires: `AppDbContext` → repositories → currency provider → exporters → services →
`MainMenu`. Nothing else.

### Menu map

```
==============================================
   DRAGON'S HOARD - ASSET TRACKING SYSTEM
==============================================

25 assets in 4 offices · 3 expiring soon · 2 past end of life

1. View assets
2. Add asset
3. Edit asset
4. Remove asset
5. Reports
0. Quit

Select option (0 - 5):
```

**View assets** (`AssetBrowser`) — one screen that replaces Week38's separate View and Search:

```
=== View Assets ===

 Id  Type     Brand    Model          Serial      Purchased    Office    Price  Code  Value (EUR)  EOL status
 ...
Page 1 of 3 · 25 assets · sorted by category
Filters: brand "dell", office Sweden, expired only

1. Previous page
2. Next page

3. Change sorting (current: Category)
4. Search and filter
5. Clear search and filters
6. Show asset details
7. Export this list
0. Back to main menu
```

- "Previous page" / "Next page" are disabled at the ends through `disabledChoices`.
- **Search and filter** opens a sub-menu; each entry shows its current value: brand contains,
  model contains, office, purchase year, category (all / computers only / mobile devices only),
  lifetime (all / expiring soon / expired only), `0. Back`. Changing anything returns to page 1.
- No matches shows a warning instead of an empty table; the menu stays so filters can be cleared.

**Edit asset** and **Remove asset** reuse `AssetBrowser` with a different ID action ("Enter
asset ID to edit" / "to remove"), the way the Milestone's `ShowTransactionsForAction` served
both. Search and filter are available there too.

- Edit: details panel, then "All fields" or one field at a time; each prompt shows the current
  value and Enter keeps it. Office is picked from a numbered list. The price prompt names the
  asset's own currency; moving an asset to another office changes neither its price nor its
  currency.
- Remove: details panel, then confirm.

**Add asset**: category → type → brand → model → serial number → purchase date → warranty
expiration → office → price in that office's currency ("Price in SEK: ") → assigned to (Enter
to skip). `0` cancels at the menu steps,
`q` at the text steps.

**Reports**: `1. Value and count per office`, `2. Assets close to expiration`,
`3. Most expensive assets`, `0. Back`. Each report screen offers `1. Export this report`, `0. Back`.

### Spectre.Console

Spectre draws, `ConsoleInput` asks.

| Piece | Spectre widget |
|---|---|
| Asset list, report tables | `Table` (simple border, right-aligned numeric columns) |
| One asset, every field | `Panel` wrapping a two-column `Grid` |
| Screen heading | `Rule`, left-justified |
| Main banner | `Panel` (or `FigletText` if it looks good at 80 columns) |
| Success / warning / error | `MarkupLine` through `ConsoleMessage` |

- **Escape all data** with `Markup.Escape` before it goes into markup — brand, model, serial
  number, search terms, file paths, exception messages.
- **No Spectre prompts and no live displays** (`Status`, `Progress`, `Live`). Prompts would
  replace the numbered menus; live displays redraw in place and don't work with paced output.
- `ConsoleMessage` keeps the Milestone's method names (`DisplayErrorMessage`, `Heading`,
  `MainHeading`, `WriteColoredSlow`, …) so `ConsoleInput` and the menu code copy over with only
  a namespace change; only the bodies become Spectre calls.
- `ClearScreen` stays as in the Milestone: clear the viewport, then `\x1b[3J` for the scrollback.

**To verify in build step 2:** that Spectre output is paced by `SlowConsole` and still coloured.
`SlowConsole.Install()` replaces `Console.Out`; Spectre picks up its output writer the first
time `AnsiConsole` is used, so `Install()` must come first. If Spectre detects the wrapped
writer as "not a terminal" and drops colour, create the console explicitly with
`AnsiConsoleSettings` (forcing ANSI on) — check the current API on https://spectreconsole.net.

### Colours (`UI/Theme.cs`)

| Use | Colour |
|---|---|
| Intro art and text, main banner, table headers | `DarkYellow` |
| Screen headings | `DarkCyan` |
| Success / warning / error | `Green` / `Yellow` / `DarkRed` |
| Lifetime: Yellow / Red / DarkRed | `DarkYellow` / `Red` / `DarkRed` |

Defined as `ConsoleColor` values and converted with `Color.FromConsoleColor`, so they match
Week38 and the Milestone exactly. Lifetime colour applies to the purchase-date cell and the
status label only.

### UI rules

- `ConsoleInput` never returns invalid data; it re-prompts until the input parses and is in range.
- `UserCancelledException` unwinds a flow when the user types `q` or input ends; it is caught
  close to where it happened so the user stays on the list they were browsing.
- Prices: input accepts `,` and `.`; display is whole numbers with thousand separators and the
  currency code in its own column. Dates: `yyyy-MM-dd`.
- No business logic in `UI/`.

---

## 10. Validation and error handling

Validation happens twice, on purpose: `ConsoleInput` re-prompts so bad input never leaves the
UI, and the domain guards its own invariants because `Core` must stay correct whoever calls it.

| Rule | Violation |
|---|---|
| Brand, model, serial number not blank, within max length | `ArgumentException` |
| Assigned to within max length (blank means none) | `ArgumentException` |
| Price > 0 and ≤ 150 000 000 | `ArgumentOutOfRangeException` |
| Purchase date not in the future | `ArgumentOutOfRangeException` |
| Warranty expiration not before purchase date | `ArgumentOutOfRangeException` |
| Serial number unique | `DuplicateSerialNumberException` |
| Asset ID exists | `AssetNotFoundException` |
| Office exists | `OfficeNotFoundException` |
| Database unreachable or a save fails | `DataStoreException` |
| Export file can't be written | `IOException` / `UnauthorizedAccessException`, caught in the export flow |

The UI checks serial-number uniqueness while prompting (through the service) so the user is
re-prompted instead of losing a half-filled form. Nothing catches `Exception` broadly.

---

## 11. Reuse map

### From the Milestone project (`CsharpMilestoneProject/src/MoneyTracker.ConsoleApp/`)

| File | Becomes | Change |
|---|---|---|
| `UI/SlowConsole.cs` | `UI/SlowConsole.cs` | Namespace only |
| `UI/UserCancelledException.cs` | same | Namespace only |
| `UI/ConsoleInput.cs` | same | Namespace; `TryRun` also catches `DataStoreException` |
| `UI/ConsoleMessage.cs` | same | Same method names, bodies rewritten on Spectre; drop `sv-SE` |
| `UI/TransactionMenu.cs` | `UI/AssetBrowser.cs` | The pagination loop, `disabledChoices`, `ApplyChange`, `PromptIdAndRunAction` patterns |
| `UI/MainMenu.cs` | `UI/MainMenu.cs` | Structure; menu items and status line change |
| `UI/TransactionTable.cs` | — | Not copied; Spectre `Table` replaces hand-padded columns |
| `Program.cs` | `Program.cs` | Structure of intro / wiring / outro (text comes from Week38) |
| `Directory.Build.props` | same | Unchanged |
| `Core/Exceptions/DataStoreException.cs` | `Core/Exceptions/` | Namespace only |

### From Week38 (`../Week38/src/AssetTracker/`)

| File | Becomes | Change |
|---|---|---|
| `Models/Asset.cs` | `Core/Models/Asset.cs` | Remove JSON attributes; `DateTime` → `DateOnly`; `PriceEur` → `PurchasePrice` + `Currency`; add serial number, warranty, assigned to; validation; status via `AssetLifecycle` |
| `Models/Computer.cs` | `Core/Models/ComputerAsset.cs` | Rename |
| `Models/MobilePhone.cs`, `Tablet.cs` | `Core/Models/MobileAsset.cs` | Merged, with `MobileType` |
| `Models/Office.cs` | `Core/Models/Office.cs` | Record with static instances → entity; instances become seed rows |
| `Models/Enums/*` | `Core/Enums/` | Namespace only |
| `Exceptions/AssetNotFoundException.cs` | `Core/Exceptions/` | Namespace only |
| `Services/ICurrencyProvider.cs` | `Core/Abstractions/` | Namespace only |
| `Services/HardcodedCurrencyProvider.cs`, `ApiCurrencyProvider.cs`, `Enums/CurrencyRateSource.cs` | `Infrastructure/Currency/` | Namespace only |
| `Services/AssetService.cs` | `Core/Services/AssetService.cs` | Sorting and search move into `AssetQuery` + repository; `GetLocalPrice` (multiply) becomes `GetValueInEur` (divide); CSV code moves out |
| `Services/AssetService.ExportToCsv`, `EscapeCsvField` | `Infrastructure/Export/CsvReportExporter.cs` | Writes a `Report` instead of assets |
| `Services/AssetsPage.cs` | `Core/Models/PagedResult.cs` | Generic, adds `TotalCount` |
| `Services/Enums/AssetSortMode.cs` | `Core/Enums/` | `AssetType` → `Category`, which becomes the default |
| `Services/IAssetRepository.cs` | `Core/Abstractions/` | Redesigned for queries (§6) |
| `Services/AppPaths.cs` | `Infrastructure/AppPaths.cs` | One more level up, into `AssetTracker.Infrastructure/Data` |
| `Services/FormatHelpers.cs` | `UI/InputValidators.cs`, `UI/Format.cs` | Split; `DateTime` → `DateOnly` |
| `UI/ConsoleTableRenderer.cs` | `UI/AssetTable.cs` | Lifetime colour switch kept; price columns become Price / Code / Value (EUR); drawing by Spectre |
| `UI/AssetMenu.cs` | `UI/AssetMenu.cs` | Add / edit / remove prompts, re-pointed at `ConsoleInput` and the services |
| `Program.cs` | `Program.cs` | Dragon art, welcome and farewell text, exchange-rate status switch |
| `Data/assets.json` | `Infrastructure/Persistence/SampleData.cs` | Source of the sample assets; prices restated in local currency |

**Not ported:** `OutputTracker.cs`, `JsonAssetRepository.cs`, `OfficeJsonConverter.cs`,
`DateOnlyJsonConverter.cs`, `DuplicateAssetIdException.cs`, `InvalidAssetDataException.cs`,
`GlobalSuppressions.cs`, Week38's `SlowConsole.cs` and `ConsoleHelpers.cs` (the Milestone
versions replace them), and "Change asset type".

### From the EF example (`EntityFrameworkCore/`)

`AppDbContext.cs` — configuration builder, `APP_DB_CONNECTION` with fallback, Fluent API style.
`ConsoleAppEF1.csproj` — package versions.

---

## 12. Build plan (one step = one commit)

Each step ends with a clean `dotnet build` (warnings are errors) and the check in the last
column. The `week-46:` prefix keeps these commits apart from the other weeks in the shared repo.

| # | Step | Check before committing | Suggested commit |
|---|---|---|---|
| | **Setup** | | |
| 0 | `CLAUDE.md`, `docs/PROJECT_CONTEXT.md`, first version of `README.md`, and the repo-level `../README.md` | — | `week-46: add project docs` |
| 1 | Replace the placeholder project: `Directory.Build.props`, `Week46.slnx`, three projects under `src/`, project references, packages, `Data/exports/` in `.gitignore` | Solution builds; `dotnet run` starts an empty app | `week-46: add solution skeleton` |
| 2 | UI foundation: copy `SlowConsole`, `ConsoleInput`, `UserCancelledException`; `Theme`; `ConsoleMessage` on Spectre; intro and outro in `Program.cs`; main menu with only `0. Quit` | Intro types slowly **and** in colour; heading clears the screen; `0` quits | `week-46: add console ui foundation` |
| | **Level 1** | | |
| 3 | Core domain: enums, `Office`, `Asset` (with `PurchasePrice` + `Currency`), `ComputerAsset`, `MobileAsset`, `AssetDetails`, `AssetLifecycle`, exceptions, validation | Builds; `Core` has no package or project reference | `week-46: add domain model` |
| 4 | `AppDbContext`, Fluent configuration, office seed, first migration, `DatabaseInitializer` | Database `asset_tracker_ef` appears in SSMS with `Assets`, `Offices` (4 rows) | `week-46: add database context and initial migration` |
| 5 | `IAssetRepository` / `IOfficeRepository` + EF implementations (add, list), `AssetService` (add, list), `SampleData` | First run inserts 25 assets; second run inserts none | `week-46: add repositories and asset service` |
| 6 | `AssetTable`, "Add asset" flow, "View assets" as a plain list | Add a computer and a phone; both survive a restart and show their price in the office's currency | `week-46: add asset registration and listing` |
| | **Level 2** | | |
| 7 | `AssetQuery`, `PagedResult`, `AssetBrowser` with pagination, sort modes, lifetime colours | Prev/Next disabled at the ends; default order is category then purchase date; all three colours visible | `week-46: add paginated asset browser` |
| 8 | `AssetDetailsPanel`, "Show asset details", "Edit asset" | Edit one field and all fields; Enter keeps the value; `q` cancels without saving | `week-46: add asset editing` |
| 9 | "Remove asset" with confirmation | Removed asset is gone after restart; unknown ID shows a message | `week-46: add asset removal` |
| | **Level 3** | | |
| 10 | `ICurrencyProvider`, both providers copied from Week38, startup status, `GetValueInEur`, Value (EUR) column | One price converts correctly when checked by hand; live rates online; warning + cached or hardcoded rates with the network off | `week-46: add currency conversion` |
| 11 | `Report`, `ReportService`, `ReportTable`, Reports menu with value and count per office | One office's EUR total matches a manual conversion of its assets | `week-46: add office report` |
| 12 | Reports: close to expiration, most expensive | Only Yellow and Red assets in the first; second is ordered by EUR value across currencies (40 000 TRY ranks below 1 500 EUR) | `week-46: add expiration and price reports` |
| | **Level 4** | | |
| 13 | Search: brand, model, office, purchase year | Criteria combine; no matches shows a warning; clearing restores the list | `week-46: add asset search` |
| 14 | Filters: expired only, computers only, mobile only; apply the browser to Edit and Remove | Each filter alone and combined with a search; page resets to 1 | `week-46: add asset filters` |
| 15 | `IReportExporter`, `ExportService`, CSV exporter, export flow on reports and list | File opens in Excel; overwrite asks first; bad file name re-prompts | `week-46: add csv export` |
| 16 | TXT and JSON exporters | JSON parses; TXT columns line up | `week-46: add txt and json export` |
| | **Finish** | | |
| 17 | Polish: main-menu status line, empty states, wording, database-down message | Stop SQL Server and start the app: clear message, no stack trace | `week-46: polish messages and empty states` |
| 18 | Full manual test against §13, fix what it finds | Every box in §13 ticked | `week-46: fix issues from final test` |
| 19 | Finish `README.md`: remove the "in progress" note and Progress table, add screenshots, correct anything that changed during the build; drop "(in progress)" from `../README.md` | A fresh clone runs by following the README | `week-46: finish readme` |

Notes on the order:

- Offices are a table from step 3 even though the brief introduces them in Level 3. An asset
  needs an office from the first insert; adding the relationship later would mean migrating
  existing rows.
- The same goes for serial number, warranty date, assigned user and the price's currency: they
  are in the first migration, so later levels add no columns. Levels 1 and 2 show the stored
  price with its currency code; the value in EUR arrives with step 10. If something does need to change, it gets its own
  migration and its own commit.
- Step 2 comes before any domain code on purpose: it is the one place where three borrowed
  pieces (Spectre, `SlowConsole`, clear-screen) meet for the first time.
- A level is done when its checks pass **and** the earlier levels still work. Update the
  Progress table in `README.md` in the commit that finishes a level.

---

## 13. Definition of done

- [ ] Levels 1–4 in §1 all ticked
- [ ] Fresh machine: with SQL Server Express running, `dotnet run` creates the database, seeds
      it and shows the menu — no manual SQL
- [ ] Add, list, sort, search, filter, edit, remove, report, export; quit, restart, data intact
- [ ] Every prompt survives empty input, letters where numbers are expected, a future purchase
      date, a warranty date before purchase, a duplicate serial number, and `q`
- [ ] Text containing `[`, `]` or a comma in brand/model displays and exports correctly
- [ ] Moving an asset to another office keeps its price and currency; only its EUR value
      follows the exchange rate
- [ ] Network off: app starts with a warning and fallback rates
- [ ] Database off: clear error, clean exit
- [ ] `dotnet build` with zero warnings; `dotnet format` leaves nothing to change
- [ ] No `Console`/`AnsiConsole` outside `UI/`, no EF types outside `Infrastructure`, no
      `File`/`HttpClient` outside `Infrastructure`, `Core` has no references
- [ ] No duplicated code between View / Edit / Remove lists or between the three exporters
- [ ] No secrets in the repo; `Data/exports/` and `exchangeRates.json` not committed
- [ ] README explains setup and shows screenshots
- [ ] `Week36/`, `Week37/`, `Week38/` untouched

---

## 14. Later, if there is time

Not in scope now. Listed so today's design doesn't block them.

**Level 5.**

- *Employees and assignment* — an `Employee` entity (ID, full name, department, email); the
  nullable `AssignedTo` string becomes a nullable foreign key in one migration.
- *Roles and simulated login* — a `User` with a role (Admin / Manager / Employee); `MainMenu`
  shows or disables entries by role, using the same `disabledChoices` mechanism.
- *Maintenance tracking* — last and next maintenance date plus notes, either columns on `Asset`
  or a `MaintenanceRecord` table.
- *Dashboard* — total assets, total value, expiring assets, assets per employee, most used
  type. The main-menu status line is its starting point; `ReportService` gets the queries.

**The brief's optional suggestions.**

- *Async database operations* — would touch every repository and service signature and the
  synchronous menu loop; no user-visible benefit in a single-user console app.
- *Logging* — EF Core's `LogTo` pointed at a file in `Data/`, never the console.
- *Configuration files* — the connection string already comes from user secrets or an
  environment variable; an `appsettings.json` would only add page size and export folder.
- *Unit tests* — `Core` depends only on interfaces, so services can be tested with in-memory
  fakes, as planned in the Milestone project.
