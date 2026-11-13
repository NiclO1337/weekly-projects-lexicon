namespace AssetTracker.Core.Models;

/// <summary>One page of a longer list.</summary>
/// <typeparam name="T">The item type.</typeparam>
/// <param name="Items">The items on this page.</param>
/// <param name="PageNumber">This page's 1-based number; always between 1 and <paramref name="TotalPages"/>.</param>
/// <param name="TotalPages">The number of pages; at least 1, even when the list is empty.</param>
/// <param name="TotalCount">The number of items across all pages.</param>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int PageNumber, int TotalPages, int TotalCount);
