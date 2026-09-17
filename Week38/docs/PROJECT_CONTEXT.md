# Asset Tracking System — Project Context

## 1. Overview
A C# console application (target: **.NET 10 LTS**) that helps a company track its
IT assets (laptops, desktops, mobile phones, tablets) across offices in different
countries, with currency-aware pricing and end-of-life warnings.


## 2. Business Rules
- All company assets have a lifespan of **3 years** from purchase date.
- End-of-life highlighting (drives both the colored `PurchaseDate` cell and
  the "EOL Status" column's text label in the View Assets table):
  - **(none)** → more than 6 months remaining — no color, blank label
  - **YELLOW** ("Monitor") → less than 6 months remaining
  - **RED** ("Upgrade soon") → less than 3 months remaining
  - **DARK RED** ("End of life") → already past the 3-year lifespan
- Assets belong to an office; each office has its own currency.
- Known offices: **Germany (EUR)**, **Sweden (SEK)**, **USA (USD)**, **Turkey (TRY)**.
- Prices are always entered in EUR and converted to local currency.

## 3. Scope Decisions
| Topic | Decision |
|---|---|
| .NET version | .NET 10 (LTS) |
| Currency conversion | Hardcoded rates as default/fallback, with an optional live API provider |
| Computer subtypes (Laptop/Desktop) | Single `Computer` class with a `ComputerType` enum property (not separate classes) |
| Tablet | Included as a full first-class asset type from the start |
| Bonus features in scope | Search by brand/model, Edit asset, Export to CSV, Pagination, Exception handling |
| Bonus features out of scope (for now) | Full colorful console UI overhaul (theming/beautifying every screen), Unit tests |
| Persistence | JSON file (`assets.json`) via `System.Text.Json`, loaded on startup, saved on every change |

## 4. Domain Model

### Asset hierarchy (abstract base + inheritance + polymorphism)
```
Asset (abstract)
├── Computer          (adds ComputerType: Laptop | Desktop)
├── MobilePhone
└── Tablet
```

**`Asset` (abstract) common properties:**
- `Id` (int, unique, auto-incremented, persisted)
- `Brand` (string)
- `Model` (string)
- `PurchaseDate` (DateTime)
- `PriceEur` (decimal)
- `Office` (Office)

**`Asset` common behavior:**
- `GetAgeInDays()` / age calculation via `DateTime`/`TimeSpan`
- `GetEndOfLifeStatus()` → `EndOfLifeStatus` (None/Yellow/Red) based on the 3-year lifespan rule
- `GetLocalPrice(ICurrencyProvider)` → converts `PriceEur` into the office's currency
- `abstract string GetCategoryLabel()` — overridden per subclass to return the display
  type shown in the "Type" column (e.g. `Computer` returns `ComputerType.ToString()`
  → "Laptop"/"Desktop"; `MobilePhone` returns "Phone"; `Tablet` returns "Tablet").
  This is the polymorphism hook used everywhere assets are displayed or sorted by type.

### Supporting types
- **`Office`** — `Name`, `Country`, `Currency` (CurrencyCode). Predefined static
  instances for Germany/Sweden/USA/Turkey; designed so more offices can be
  added later without touching Asset logic.
- **`ComputerType`** enum — `Laptop`, `Desktop`
- **`CurrencyCode`** enum — `EUR`, `SEK`, `USD`, `TRY`
- **`EndOfLifeStatus`** enum — `None`, `Yellow`, `Red`

### Custom exceptions
- `DuplicateAssetIdException` — thrown when adding/importing an asset whose ID
  already exists
- `AssetNotFoundException` — thrown when editing/removing/searching for a
  non-existent asset
- `InvalidAssetDataException` — thrown on malformed input or corrupted file data

## 5. Sorting Requirements
- Sort by **Office** (a-z), then **Purchase Date** - default
- Sort by **Asset Type** (a-z), then **Purchase Date**
- Sort by **End of life**, (oldest to newest)
- Available from the menu (user picks sort mode)

## 6. Currency Conversion
- `ICurrencyProvider.GetRate(CurrencyCode)` is the single abstraction used everywhere
  prices are converted. `AssetService.GetLocalPrice(Asset, ICurrencyProvider)` is the one
  place the multiplication (`PriceEur * GetRate(Office.Currency)`) happens - it's a static
  method on `AssetService`, not on `Asset` itself, so `Models` never has to reference
  `Services` (`ICurrencyProvider` lives in `Services`, alongside `IAssetRepository`).
- `HardcodedCurrencyProvider` — fixed dictionary of EUR→{SEK,USD,TRY} rates (EUR is base,
  rate 1.0). Approximate/offline only; not kept in sync with real rates over time.
- `ApiCurrencyProvider` — calls [Frankfurter](https://frankfurter.dev/) (free, no API key,
  ECB reference rates, `base=EUR` matches how `PriceEur` already works):
  `GET https://api.frankfurter.dev/v1/latest?base=EUR&symbols=SEK,USD,TRY`.
  - Caches the result in a single file, `Data/exchangeRates.json`, containing the fetched
    `date` and `rates`. On construction, if the cached `date` equals today, the cache is
    used and the API is never called; otherwise it fetches live and overwrites the cache.
    This means at most one API call per calendar day, persisting across app restarts -
    important for repeated manual testing.
  - On any failure (network error, bad response, timeout, unreadable/corrupt cache) it
    **falls back to `HardcodedCurrencyProvider`** rather than crashing or blocking the
    app, per the "Do NOT let it throw" rule in `CLAUDE.md`. Since `Services` can't touch
    the console, it exposes `UsedFallback` instead of printing anything itself - the
    caller (`Program.cs`, the composition root) checks this once at startup and shows a
    warning via `ConsoleHelpers` if it's true.
  - `Data/exchangeRates.json` is gitignored (live-fetched cache, not seed data) - unlike
    `Data/assets.json`, which is committed.
- A startup prompt in `Program.cs` (before the main menu loop) lets the user pick
  "Hardcoded" (default - pressing Enter selects it) or "Live API".
- View Assets / Search Assets / the Edit-Remove asset-picker table all show two extra
  columns after Office: **Local Value** (`GetLocalPrice`, formatted like the EUR price
  column) and **Currency** (`Office.Currency`). CSV export adds the same two columns
  (`LocalValue`, `Currency`) between `Office` and `EndOfLifeStatus`.

## 7. Persistence (Level 5)
- File: `Data/assets.json`, resolved via `AppPaths.DataDirectory` (walks up from the build
  output to the project folder) so it always lands at `src/AssetTracker/Data` regardless of
  how the app is launched (`dotnet run`, IDE debug, or the built `.exe` directly).
- Uses `System.Text.Json` with polymorphic serialization
  (`[JsonPolymorphic(TypeDiscriminatorPropertyName = "assetType")]` / `[JsonDerivedType]` on
  `Asset`) so Computer/MobilePhone/Tablet round-trip correctly. **The `assetType` discriminator
  property must be the first key in each JSON object** - System.Text.Json's polymorphic reader
  requires this and throws otherwise.
- `Office` serializes as its plain `Name` string (via `OfficeJsonConverter`), not a nested
  object - looked up against `Office.All` on read.
- `PurchaseDate` serializes as a plain `yyyy-MM-dd` string (via `DateOnlyJsonConverter`),
  matching the format used everywhere else in the app.
- Enums (`ComputerType`) serialize as their string name (`JsonStringEnumConverter`), not a
  numeric value.
- On startup: load all assets, determine the next available `Id`. If the file is corrupted,
  show a warning and start with an empty list rather than crashing (mirrors the planned
  `ApiCurrencyProvider` fallback behavior in §6).
- On every add/edit/remove: re-save the full file (auto-save, no explicit "save" step
  needed from the user).
- Duplicate IDs must never be written to the file — checked before every write.
- The committed `Data/assets.json` seed data is intentional (not test cruft) - the teacher
  requires sample data to be present when the project starts. CSV exports from the
  "Export to CSV" feature are gitignored instead, since those are on-demand/regenerable.

Example `assets.json` entry:
```json
{
  "assetType": "Computer",
  "id": 1,
  "computerType": "Laptop",
  "brand": "Apple",
  "model": "MacBook Pro",
  "purchaseDate": "2024-03-15",
  "priceEur": 1500,
  "office": "Sweden"
}
```

## 8. Bonus Features In Scope
| Feature | Notes |
|---|---|
| Search by brand/model | Case-insensitive partial match, returns list |
| Edit asset | By ID; re-validates and re-saves |
| Export to CSV | Dumps current (optionally filtered/sorted) asset list to a `.csv` file |
| Pagination | Console "View Assets" screen shows N rows per page with next/prev |
| Exception handling | All user input and file I/O wrapped in try/catch with clear console messages; custom exceptions used for domain errors |

## 9. Menu (target shape)
```
================================================
COMPANY ASSET TRACKING SYSTEM
================================================
1. Add Asset
2. View Assets (paginated)
3. Search Assets (by brand/model)
4. Edit Assets
5. Remove Asset
6. Export to CSV
7. Exit
Select option:
```

## 10. Future potential features
- A full colorful console UI overhaul — theming/beautifying every screen.
  **Not the same as small, targeted color use** (e.g. `ConsoleHelpers`'s
  error/warning/success messages, or coloring just the `PurchaseDate` cell
  red/yellow when an asset is near end-of-life) — that kind of helper
  coloring is in scope any time and doesn't need to wait for this. Tables
  otherwise stay plain formatted text.
- Automated unit tests
- Support for offices/currencies beyond Sweden/USA/Turkey (architecture should make adding one easy, but no UI for managing offices is required)
- Complex freetext search — a single search box that splits input into words and
  matches each word against both Brand and Model (OR across fields/words), instead
  of the current "pick Brand or Model, then enter one term" flow. Needs a relevance
  ranking strategy (e.g. items matching more words should outrank single-word
  matches) to stay usable once results start piling up.
