using AssetTracker.Core.Enums;

namespace AssetTracker.Core.Models;

/// <summary>
/// Which assets to list and how: sort order and page. Immutable; change it with a
/// <see langword="with"/> expression, e.g. <c>query with { Page = 2 }</c>.
/// </summary>
public sealed record AssetQuery
{
    /// <summary>The number of assets per page unless another size is given.</summary>
    public const int DefaultPageSize = 10;

    /// <summary>The order of the list. Defaults to <see cref="AssetSortMode.Category"/>.</summary>
    public AssetSortMode SortBy { get; init; } = AssetSortMode.Category;

    /// <summary>The 1-based page to return. A page past the end returns the last page.</summary>
    public int Page { get; init; } = 1;

    /// <summary>The number of assets per page.</summary>
    public int PageSize { get; init; } = DefaultPageSize;
}
