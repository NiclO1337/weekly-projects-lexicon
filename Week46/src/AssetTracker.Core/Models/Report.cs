namespace AssetTracker.Core.Models;

/// <summary>
/// A titled table of values, the one shape every report has, so one screen renderer and one
/// exporter per file format cover them all. Cells hold raw values (<see cref="string"/>,
/// <see cref="int"/>, <see cref="decimal"/>, <see cref="DateOnly"/>), not formatted text, so each
/// output can format them its own way.
/// </summary>
/// <param name="Title">What the report shows.</param>
/// <param name="Columns">The columns, in order.</param>
/// <param name="Rows">One list of cell values per row, in column order.</param>
public sealed record Report(string Title, IReadOnlyList<ReportColumn> Columns, IReadOnlyList<IReadOnlyList<object>> Rows);
