using System.Text.Json.Serialization;

namespace MyAi.Application.Common.Models;

/// <summary>Paginated response wrapper.</summary>
public class PaginatedList<T>
{
    public PaginatedList(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
        Items = items;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        HasNextPage = PageNumber < TotalPages;
        HasPreviousPage = PageNumber > 1;
    }

    public IReadOnlyList<T> Items { get; }

    public int PageNumber { get; }

    public int PageSize { get; }

    public int TotalCount { get; }

    public int TotalPages { get; }

    public bool HasNextPage { get; }

    public bool HasPreviousPage { get; }

    [JsonIgnore]
    public bool IsEmpty => Items.Count == 0;

    public static PaginatedList<T> Empty(int pageNumber, int pageSize) =>
        new(Array.Empty<T>(), 0, pageNumber, pageSize);
}
