# Dragon's Hoard – Product Management System

A C# console application for managing a product catalog.

## Features

**Products**
- Add, edit (name / price / category), and delete products
- Each product gets a unique, auto-incrementing ID
- Show products in a formatted table, sorted by price (low→high, high→low) or by category
- Search by product name or category, with matches highlighted in green
- Input validation for name length and price (positive numbers, comma-decimal, upper limit)

**Categories**
- Add, edit, and delete categories
- Deleting a category is blocked while products still reference it
- Categories can be created on the fly while adding/editing a product

**Statistics**
- Most expensive and cheapest product
- Average product price
- Product count per category

**Data Persistence**
- Save all products and categories to `data.json`
- Load previously saved data (with confirmation, since it overwrites what's in memory)
- Reset in-memory data (products only, or products + categories) without touching the saved file
- Optional save prompt on exit

**UX / Error Handling**
- Numbered menu system for all operations
- Type `q` at most prompts to cancel and return to the previous menu (`UserCancelledException`)
- Confirmation prompts before destructive actions (delete, reset, load, overwrite)
- Centralized input validation and colored success/warning/error messages

## Project Structure

| File | Responsibility |
|---|---|
| `Program.cs` | Entry point, intro/outro banners, wiring up managers |
| `MainMenu.cs` | Top-level menu loop |
| `ProductMenu.cs` / `CategoryMenu.cs` | Menu flows for product and category operations |
| `DataMenu.cs` | Save / Load / Reset / Exit handlers |
| `Product.cs` / `Category.cs` | Domain models |
| `ProductManager.cs` / `CategoryManager.cs` | Business logic, LINQ queries (`OrderBy`, `Where`, `Sum`, `FirstOrDefault`, `GroupBy`) |
| `DataStore.cs` / `AppData.cs` | JSON serialization to/from `data.json` |
| `Utils.cs` | Shared input validation, formatting, and console helpers |
| `OutputTracker.cs` | Tracks console output to control menu-pause behavior |
| `UserCancelledException.cs` | Signals a user-initiated cancel (`q`) |

## Requirements

- .NET SDK (8.0 or later recommended)

## Running the App

On first run there's no `data.json` yet — start by adding a category and a few products, or explore the menu. Use **Save Data** before exiting if you want your changes to persist between sessions.

## Notes / Possible Extensions

Not yet implemented from the optional bonus list: pagination, price-range filtering, exporting statistics to a text file, and admin/user modes. The current architecture (separate manager classes, JSON persistence) should make these straightforward to add later.
