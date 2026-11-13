namespace AssetTracker.Core.Models;

/// <summary>A column of a <see cref="Report"/>.</summary>
/// <param name="Header">The column heading.</param>
/// <param name="IsNumeric">Whether the values are numbers, which are right-aligned on screen.</param>
public sealed record ReportColumn(string Header, bool IsNumeric = false);
