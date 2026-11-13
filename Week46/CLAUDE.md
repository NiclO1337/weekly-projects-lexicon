# CLAUDE.md

Console asset tracker ("Dragon's Hoard") for company IT assets across offices and currencies.
.NET 10 (LTS), C# 14, Entity Framework Core 10 on SQL Server Express, Spectre.Console for
output. School project (Weekly Mini Project, week 46).
**Target: brief levels 1–4. Level 5 only when asked.**

This is Week38's AssetTracker rebuilt on a database, with the UI upgrades and the
Core / Infrastructure / ConsoleApp split from the Milestone project (MoneyTracker).

Full design and build plan: `docs/PROJECT_CONTEXT.md` — read it before making design decisions.
It is the source of truth: if it and this file disagree, the context doc wins — and say so, so
one of them gets fixed.
**Work follows the build plan in §12: one step = one commit, in order. Don't jump ahead.**

## Where things come from

Copy and adapt; do not rewrite from scratch what already exists. `PROJECT_CONTEXT.md` §11 has
the file-by-file map.

| Need | Copy from | Adapt |
|---|---|---|
| Asset rules, sorting, search, CSV export, currency providers, intro text | `../Week38/src/AssetTracker/` (same repo) | Split across layers, JSON repository → EF Core |
| `ConsoleInput`, `SlowConsole`, `UserCancelledException`, paginated menu pattern, `Directory.Build.props` | Milestone project `CsharpMilestoneProject/` (separate repo; a copy is in the claude.ai Project) | Rename namespaces, drop SEK/`sv-SE` specifics, `ConsoleMessage` and tables rewritten on Spectre.Console |
| `AppDbContext` pattern, package versions | `../../EntityFrameworkCore/AppDbContext.cs`, `ConsoleAppEF1.csproj` | New database name, no console logging |

Never modify `Week36/`, `Week37/` or `Week38/` — they are handed-in work. The git root is the
parent folder (`WeeklyMiniProjects/`); everything for this project lives under `Week46/`.

## Commands

Run from `Week46/`.

```bash
dotnet build                                   # build all
dotnet run --project src/AssetTracker.ConsoleApp   # the user runs this, never Claude
dotnet format                                  # fix style before committing
dotnet list package --vulnerable               # after adding or updating a package

# Migrations (Infrastructure owns them, ConsoleApp is the startup project)
dotnet ef migrations add <Name> --project src/AssetTracker.Infrastructure --startup-project src/AssetTracker.ConsoleApp
dotnet ef database update      --project src/AssetTracker.Infrastructure --startup-project src/AssetTracker.ConsoleApp
```

Visual Studio Package Manager Console equivalent:
`Add-Migration <Name> -Project AssetTracker.Infrastructure -StartupProject AssetTracker.ConsoleApp`.

Warnings are errors. A build with warnings is a failed build.

Run the checks a change can affect (`dotnet build`, `dotnet format`) before suggesting a
commit, and report the result honestly, failures included. **Never run `dotnet run`** — the
app waits in interactive menus and never exits. Say what to check and the user runs it.

## How we work together

- **One build-plan step at a time.** Say which step you are on. When it is done, say what
  changed, why, and what to check by hand — then **stop and wait** before the next step.
  Don't mix unrelated changes (a refactor and a feature, a fix and a formatting pass).
- **Look at the whole picture first.** Before changing something, find where else it is used
  and what depends on it.
- **Fix root causes.** If a fix feels complicated, or needs a second fix on top of the first,
  stop, explain the cause and suggest a different approach. Ask before a larger refactor.
- **Explain new concepts.** The first time a technique shows up in this project, explain it in
  one or two plain sentences. The user wants to understand every line.
- **Ask before anything destructive**: `dotnet ef database drop`, `dotnet ef migrations remove`,
  deleting files, the rate cache or exports.
- No debug output, commented-out code, unused files or unused packages left behind.
- Don't invent data, results or content. Ask, or leave a `TODO`.

## Git and commits

- **The user makes every commit.** Never run `git add`, `commit`, `push`, `merge`, `rebase`,
  `reset`, `stash`, or create/switch/delete branches. Read-only git (`status`, `diff`, `log`) is fine.
- **Always suggest a commit message** when a step is done: short (under ~50 characters),
  imperative, lowercase, prefixed `week-46:` — e.g. `week-46: add ef repositories`.
  §12 lists the suggested message for each step.
- All work is on `main`.
- If a request doesn't fit in one commit, say so and suggest how to split it.

## Architecture

```
ConsoleApp ──► Core ◄── Infrastructure
     └────────────────────►┘
```

- `AssetTracker.Core` — models, enums, repository/provider/exporter interfaces, services,
  exceptions. **References no project and no NuGet package** (no EF Core, no Spectre here).
- `AssetTracker.Infrastructure` — `AppDbContext`, `Migrations/`, EF repositories, seeding,
  currency providers (HTTP + cache file), TXT/CSV/JSON exporters. References `Core`.
- `AssetTracker.ConsoleApp` — `Program.cs` (composition root only) and `UI/`. References both.

**Never add a project reference that points the other way.** If a layer seems to need one, the
design is wrong — stop and say so.

## Hard rules

- **No `Console.*` or `AnsiConsole.*` outside `src/AssetTracker.ConsoleApp/UI/`** (and the intro
  in `Program.cs`, which goes through `ConsoleMessage`).
- **No EF Core types outside `Infrastructure`.** No `DbContext`, `DbSet`, `IQueryable`,
  `Include` or `Microsoft.EntityFrameworkCore` usings in `Core` or `UI/`. Repositories take
  intent (enums, a query record) and return materialized `IReadOnlyList<T>`.
- **No `File.*` / `Directory.*` / `HttpClient` outside `Infrastructure`.**
- **No business logic in `UI/`** — no sorting, filtering, totals or currency math. Ask the service.
- **No business logic in `Program.cs`** — it shows the intro, wires dependencies, runs `MainMenu`.
- **`decimal` for money.** Never `double` or `float`. Every money column gets explicit precision.
- **Prices are entered and stored in local currency** (`PurchasePrice` + `Currency` on the
  asset): what the office paid, which never changes. The currency is set from the office when
  the asset is created and stays put if the asset moves office. The value in EUR is always
  computed (`PurchasePrice ÷ ICurrencyProvider.GetRate(Currency)`); it is never stored and never
  hardcoded. This deliberately reverses the brief — see `PROJECT_CONTEXT.md` §2 for why.
- **Never compare or sum prices across currencies.** Convert to EUR first. In queries: aggregate
  per currency in SQL, then convert the few resulting rows in the service.
- **Never branch on asset type for behaviour or display.** No `is ComputerAsset`, no `GetType()`.
  Add an abstract member to `Asset` and override it. (Filtering a query by category with
  `OfType<T>()` inside a repository is fine.)
- **Escape every piece of data before it reaches Spectre markup** (`Markup.Escape`). Brand,
  model, serial number, file paths and exception messages can contain `[` and will otherwise
  throw or restyle the screen.
- **Never catch `Exception` broadly.** Catch the specific type.
- **Never commit a real connection string or secret.** Only the localhost fallback lives in source.

## Domain

From the brief — each asset has: Id, asset type, brand, model name, purchase date, purchase
price, local price (computed), office, serial number, employee/username (optional), warranty
expiration date. Here the stored purchase price is the local one, and the computed figure is
its value in EUR.

- `Asset` (abstract) → `ComputerAsset` (Laptop / Desktop) and `MobileAsset` (Phone / Tablet).
  iPhone, Samsung and Nokia are brands, not classes. Abstract `Category` and `TypeLabel` are
  the polymorphic hooks for display. Category and type are fixed once an asset is created.
- `Office`: Name, Country, Currency. Germany (EUR), Sweden (SEK), USA (USD), Turkey (TRY).
  Now a database entity; an asset belongs to exactly one office. The office's currency is the
  currency of assets created there.
- **Lifetime is 3 years from purchase date.** Same statuses, labels and colours as Week38
  (the brief's yellow/red wording is a confirmed typo):

  | Remaining lifetime | `EndOfLifeStatus` | Label | Colour |
  |---|---|---|---|
  | more than 6 months | `None` | (blank) | none |
  | less than 6 months | `Yellow` | Monitor | `ConsoleColor.DarkYellow` |
  | less than 3 months | `Red` | Upgrade soon | `ConsoleColor.Red` |
  | already past 3 years | `DarkRed` | End of life | `ConsoleColor.DarkRed` |

  The thresholds live in one place in `Core` (`AssetLifecycle`); the status on an asset and the
  "expired" / "expiring soon" query filters both use it. The colour mapping lives in one place
  in `UI/`.
- Default sort: office, then purchase date. Also by category (the brief's Level 2 order: category,
  then type, then purchase date) and by end of life.

## Persistence

- EF Core, SQL Server Express, code-first with migrations. Table-per-hierarchy for `Asset`.
- Connection string: `APP_DB_CONNECTION` from user secrets or environment, falling back to
  `Server=localhost\SQLEXPRESS;Database=asset_tracker_ef;Integrated Security=true;Encrypt=false;`
  — same pattern as the EF example. `Infrastructure.csproj` needs its own `<UserSecretsId>`.
- **Do not copy the example's `LogTo(Console.WriteLine, ...)`** — SQL logs would print into the
  menus and break the no-`Console` rule.
- Schema changes go through a migration. Never `EnsureCreated()`, never edit the database by hand,
  never hand-edit `*.Designer.cs` or the model snapshot.
- Startup applies pending migrations (`Database.Migrate()`), then seeds sample assets when the
  table is empty (the teacher expects data on first run). The four offices are seeded by the
  migration. Sample purchase dates are relative to today so every lifetime status is visible.
- IDs are database identity columns. Never assign or reuse them in code.
- Configuration in Fluent API (`OnModelCreating`), not attributes on `Core` models: max lengths
  from the same constants the domain validates against, unique index on serial number, required
  office FK with `DeleteBehavior.Restrict`, enums stored as strings.
- Read-only list queries use `AsNoTracking()`. Filter, sort and page in the database with LINQ —
  do not load the whole table and filter in memory.
- Database unreachable or failing → wrap in `DataStoreException` inside `Infrastructure`.
  At startup `Program.cs` reports it and exits; mid-session the menu shows the error and returns.
  Never start with silently empty data.
- Every add/edit/delete is saved immediately. There is no manual save command.

## Currency

- `ICurrencyProvider.GetRate(CurrencyCode)` is EUR-based (EUR = 1.0), unchanged from Week38.
  Week38 multiplied EUR by the rate to get the local price; here the local price is divided by
  the rate to get EUR, in one place (`AssetService.GetValueInEur`).
- `ApiCurrencyProvider` (Frankfurter, `base=EUR`) → today's cache → stale cache →
  `HardcodedCurrencyProvider`. **It never throws**; it exposes `Source` so `Program.cs` can
  report which rates are in use.
- The rate cache is `Data/exchangeRates.json` in `Infrastructure` (already gitignored at the repo
  root). Exports go to `Data/exports/` (add to `.gitignore`).

## Validation

Validate twice: `ConsoleInput` re-prompts so bad input never leaves the UI, and the domain
guards its own invariants anyway.

| Rule | Throw |
|---|---|
| Brand, model, serial number non-blank, within max length | `ArgumentException` |
| Price > 0 and ≤ max | `ArgumentOutOfRangeException` |
| Purchase date not in the future | `ArgumentOutOfRangeException` |
| Warranty expiration not before purchase date | `ArgumentOutOfRangeException` |
| Serial number already exists | `DuplicateSerialNumberException` |
| Unknown asset ID / office | `AssetNotFoundException` / `OfficeNotFoundException` |

Invalid console input is a normal event — re-prompt, don't throw.

## UI

**Spectre.Console draws; `ConsoleInput` asks.** Output (tables, headings, panels, coloured
messages) is Spectre. Input stays the Milestone numbered menus and re-prompt loops.

- **`0` is always back / cancel / quit.** Every menu goes through
  `ConsoleInput.SelectMenuOption(menuItems, zeroLabel, disabledChoices)`; never number the exit
  option last, never hand-write a `Select option (...)` prompt.
- **No Spectre prompts** (`SelectionPrompt`, `TextPrompt`, `Confirm`) — they would replace the
  numbered menus. **No Spectre live displays** (`Status`, `Progress`, `Live`) — they redraw in
  place and fight `SlowConsole`'s paced output.
- **Pagination** uses `disabledChoices` for "Previous page" / "Next page" at the ends of the list.
  Every asset list is paginated.
- **Each screen clears first.** Start screens with `ConsoleMessage.Heading` / `MainHeading`, which
  clear the viewport and scrollback. When a result message must be read before the next screen,
  call `ConsoleInput.Pause()` (or use `TryRun`).
- **No `OutputTracker`.** Do not port it or anything that depends on it.
- **Intro:** Week38's dragon art and "Dragon's hoard" welcome/farewell text, rendered with the
  Milestone `SlowConsole` (`Install()` + typewriter text via `ConsoleMessage.WriteColoredSlow`).
  `SlowConsole.Install()` must run before the first Spectre call.
- Free-text prompts accept `q` to cancel (`UserCancelledException`, caught by `TryRun` or locally
  so the user stays on the current list).
- Edit prompts show the current value; empty input keeps it. Delete shows the asset and asks to confirm.
- **All colours are defined once in `UI/Theme.cs`** and applied through `ConsoleMessage` and the
  table renderers. Never set `Console.ForegroundColor` directly, never write a colour name in a
  menu class. Convert with `Color.FromConsoleColor(...)` — Spectre's own `Color.DarkRed` is not
  the console's dark red.
- Lifetime status colours the purchase-date cell and the status label only.
- Dates are `yyyy-MM-dd`. Amounts accept `,` and `.`; display as whole numbers with thousand
  separators plus the currency code. Don't rely on `€`/`₺` rendering — show the code. Price
  prompts name the currency being entered ("Price in SEK: ").

## Features by level

1. **Registration** — create assets, save to database, list all. Inheritance, constructors, properties.
2. **CRUD + status** — update, delete, sort by category then purchase date, lifetime highlighting,
   menu navigation, validation of dates and empty fields.
3. **Offices + currency + reports** — offices with currency, local → EUR conversion, and reports:
   total value per office, asset count per office, assets close to expiration, most expensive assets.
4. **Search, filter, export** — search by brand, model, office, purchase year; filter expired only,
   computers only, mobile devices only, by office; export the current list or report to TXT, CSV
   and JSON (confirm before overwriting).

Level 5 (roles, login, employees, maintenance tracking, dashboard) is **not in scope** — do not
build toward it until asked, but don't block it either (keep employee/username a simple nullable
string for now).

## Style

- File-scoped namespaces, one type per file, filename matches type.
- `sealed` by default. `virtual`/`abstract` only where polymorphism is actually used.
- Nullable enabled. No `!` without a comment justifying it.
- `_camelCase` private fields, `s_camelCase` static readonly.
- XML doc comments on all public members of `Core`; elsewhere comment only what isn't obvious.
- LINQ over manual loops. Small, reusable methods; no duplicated code — the brief grades on this.
- Meaningful names. No abbreviations beyond `Id`, `Eol`, `Csv`, `Json`, `Eur`.

## Packages

Exactly these, at these versions. Ask before adding anything else.

| Package | Version | Project |
|---|---|---|
| `Microsoft.EntityFrameworkCore` | 10.0.14 | Infrastructure |
| `Microsoft.EntityFrameworkCore.SqlServer` | 10.0.14 | Infrastructure |
| `Microsoft.Extensions.Configuration.EnvironmentVariables` | 11.0.0 | Infrastructure |
| `Microsoft.Extensions.Configuration.UserSecrets` | 11.0.0 | Infrastructure |
| `System.Security.Cryptography.Pkcs` | 10.0.14 | Infrastructure |
| `Microsoft.EntityFrameworkCore.Tools` | 10.0.14 | ConsoleApp (`PrivateAssets=all`) |
| `Spectre.Console` | 0.57.2 | ConsoleApp |

The EF and configuration versions are the ones in the EF example project. Spectre.Console is
pre-1.0 and its API still moves between versions — check https://spectreconsole.net before
relying on a remembered method signature. The same goes for .NET 10, C# 14 and EF Core 10:
they may be newer than what you remember. Check the official docs before writing
version-specific code, trust the compiler over memory, and don't copy patterns from older
major versions.

## Security

Security is part of every step, not a clean-up at the end. When a step touches it, say so in
the summary and say what to test. If you notice a problem in existing code, say so even when it
is outside the current step.

- **CSV export neutralises formulas.** A cell starting with `=`, `+`, `-` or `@` becomes a
  formula when the file is opened in Excel. Prefix such values so they stay text.
- **Exports stay in `Data/exports/`.** If the user names the file, strip path separators and `..`.
- **No SQL built from strings.** LINQ, or `FromSql` with interpolation — never `FromSqlRaw`
  with values concatenated in.
- **Errors shown to the user are plain.** No stack traces, connection strings, server names or
  SQL in a message. `DataStoreException` carries a friendly message; the inner exception keeps
  the details.
- **Seed data uses placeholder names** (`jdoe`, `asmith`), never real people.
- **Packages:** after adding or updating one, run `dotnet list package --vulnerable` and report
  findings instead of ignoring or force-fixing them.
- **Never weaken security to make something work.** `Encrypt=false` is acceptable only in the
  localhost fallback. If there seems to be no other way, stop and ask.
- If a secret ends up in the repo or the chat, say so right away so it can be rotated.

## Scope

Build what the brief's levels 1–4 and `docs/PROJECT_CONTEXT.md` describe. This is a school
project graded on clean OOP, EF Core and LINQ; unasked-for machinery counts against it.

Do **not** add unless asked: async database calls, a DI container, a logging framework,
AutoMapper, `appsettings.json`, a test project, a web API. (The brief lists async, logging and
configuration files as optional suggestions — they are candidates for later, not defaults.)

## Testing

There is no test project; testing is manual. Before calling a level done, the user runs it end
to end by hand: add, list, sort, search, filter, edit, remove, export, quit, restart, and
confirm the data is still there. The full checklist is the definition of done in
`PROJECT_CONTEXT.md` §13.

## README and docs

- Keep `README.md` accurate when setup or behaviour changes.
- When a section needs a screenshot that doesn't exist yet, write the image markdown anyway with
  alt text and a suggested path, plus `<!-- TODO: take screenshot -->`.
- Leave `TODO` markers for things that can only be filled in later. Don't invent them.
- A decision made during the work that isn't in `PROJECT_CONTEXT.md` goes there, not only in the chat.
