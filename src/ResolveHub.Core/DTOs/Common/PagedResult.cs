namespace ResolveHub.Core.DTOs.Common;

using System;
using System.Collections.Generic;

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    /// <summary>Maximum page size enforced by every listing stored procedure.</summary>
    public const int MaxPageSize = 100;

    private readonly int _pageSize;

    /// <summary>
    /// The effective page size. Listing procedures cap the page size at <see cref="MaxPageSize"/>,
    /// so the value is clamped here too; otherwise TotalPages/HasNextPage would be computed from
    /// the requested (larger) size and callers would silently miss rows.
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
