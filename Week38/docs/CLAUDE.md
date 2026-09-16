# CLAUDE.md

Instructions for Claude (via Claude Code) when working in this repository.
For domain rules, class design, and scope decisions, see `docs/PROJECT_CONTEXT.md`
— read it before making architectural changes.

## Project summary
A .NET 10 C# console app that tracks company IT assets (computers, phones,
tablets) across offices/currencies, with end-of-life warnings and JSON
persistence. Built incrementally through 5 levels; see PROJECT_CONTEXT.md
§9 for the target menu and §8 for in-scope bonus features.

## Tech stack & commands
- Language/runtime: C#, .NET 10 (LTS)
- Build: `dotnet build`
- Run: `dotnet run --project src/AssetTracking`
- Restore: `dotnet restore`
- No test project currently exists (unit tests are out of scope for now —
  see PROJECT_CONTEXT.md §10). Do not scaffold one unless asked.

## File structure (authoritative — keep in sync with PROJECT_CONTEXT.md §4 folder tree)
```
src/AssetTracking/
├── Program.cs          # composition root only — no business logic here
├── Models/              # Asset hierarchy, enums, Office. No file I/O, no console I/O.
├── Exceptions/          # Custom exception types
├── Services/            # Business logic, persistence, currency, CSV export
├── UI/                  # Console menu + table rendering. No business logic here.
└── Data/assets.json     # persisted state (do not hand-edit; treat as generated)
```

## Architecture rules
- **Models are dumb.** `Asset` and its subclasses hold data and asset-level
  calculations (age, EOL status, category label) — they never touch the
  console or the filesystem.
- **Services own behavior.** Sorting, searching, pagination, persistence, and
  currency conversion all live in `Services/`, behind interfaces
  (`IAssetRepository`, `ICurrencyProvider`) so implementations are swappable.
- **UI only orchestrates.** `ConsoleMenu` calls into `AssetService` and
  renders results via `ConsoleTableRenderer`. It should contain no sorting,
  filtering, or persistence logic of its own.
- **Polymorphism over type-switches.** When behavior differs by asset type
  (e.g. the "Type" column label), add/override a method on the `Asset`
  subclass rather than adding `if (asset is Computer)` branches in services
  or UI code.
- **New asset type checklist:** if a new asset type is ever added, it must:
  1. Inherit `Asset` and implement `GetCategoryLabel()`
  2. Be registered as a `[JsonDerivedType]` on `Asset` for serialization
  3. Be added to `Program.cs`'s "Add Asset" flow

## Coding conventions
- Nullable reference types enabled; avoid `null` where a default/empty value
  works instead.
- File-scoped namespaces, one type per file, filename matches type name.
- Prefer `record` for simple immutable data (e.g. `Office`) and classes for
  the `Asset` hierarchy (mutable, has behavior).
- Use `DateTime`/`TimeSpan` for all date/age math — no manual day-counting.
- All currency amounts are `decimal`, never `double`/`float`.
- Wrap user input parsing and file I/O in try/catch; surface domain errors
  via the custom exceptions in `Exceptions/`, not generic `Exception`.

## Do NOT
- Do not hardcode office/currency logic outside `Office`/`ICurrencyProvider` —
  route all currency math through `ICurrencyProvider.GetRate(...)`.
- Do not let `ApiCurrencyProvider` throw on network failure — it must catch
  and fall back to `HardcodedCurrencyProvider`.
- Do not write duplicate asset IDs to `assets.json` — always check via
  `IAssetRepository` before persisting a new asset.
- Do not put console `Console.Write*`/`Console.Read*` calls inside `Models/`
  or `Services/` — those layers must stay UI-agnostic.
- Do not change the JSON schema in `Data/assets.json` without updating
  `PROJECT_CONTEXT.md` §7 to match.

## Workflow notes
- When implementing a new feature, check `PROJECT_CONTEXT.md` first for
  whether it's in scope (§8) or future feature (§10) before building it.
- Keep `Program.cs` minimal: wire up repository → currency provider →
  services → menu, then call `menu.Run()`.
