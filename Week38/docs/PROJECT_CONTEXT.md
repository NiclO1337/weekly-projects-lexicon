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
  prices are converted.
- `HardcodedCurrencyProvider` — fixed dictionary of EUR→{SEK,USD,TRY} rates (EUR is base,
  rate 1.0).
- `ApiCurrencyProvider` — calls a live exchange-rate API; on failure (network error,
  bad response, timeout) it **falls back to `HardcodedCurrencyProvider`** rather than
  crashing or blocking the app. Incase of error, inform user about this in UI.
- The menu should let the user pick which mode to use at startup, defaulting to
  hardcoded.

## 7. Persistence (Level 5)
- File: `Data/assets.json`
- Uses `System.Text.Json` with polymorphic serialization
  (`[JsonPolymorphic]` / `[JsonDerivedType]` on `Asset`) so Computer/MobilePhone/Tablet
  round-trip correctly.
- On startup: load all assets, determine the next available `Id`.
- On every add/edit/remove: re-save the full file (auto-save, no explicit "save" step
  needed from the user).
- Duplicate IDs must never be written to the file — checked before every write.

Example `assets.json` entry:
```json
{
  "id": 1,
  "assetType": "Computer",
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
