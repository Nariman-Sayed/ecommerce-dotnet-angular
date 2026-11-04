namespace Ecom.Core.DTO;

/// <summary>
/// A single page of results together with paging metadata.
/// </summary>
/// <typeparam name="T">The element type of the page.</typeparam>
public class Pagination<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Pagination{T}"/> class.
    /// </summary>
    /// <param name="pageIndex">The 1-based page index.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="totalCount">The total number of items across all pages.</param>
    /// <param name="data">The items on this page.</param>
    public Pagination(int pageIndex, int pageSize, int totalCount, IEnumerable<T> data)
    {
        if (pageIndex < 1)
            throw new ArgumentException("Page index must be greater than or equal to 1.", nameof(pageIndex));

        if (pageSize < 1)
            throw new ArgumentException("Page size must be greater than or equal to 1.", nameof(pageSize));

        if (totalCount < 0)
            throw new ArgumentException("Total count must be greater than or equal to 0.", nameof(totalCount));

        if (data is null)
            throw new ArgumentNullException(nameof(data));

        PageIndex = pageIndex;
        PageSize = pageSize;
        TotalCount = totalCount;
        Data = data.ToList().AsReadOnly();
    }

    /// <summary>
    /// Gets the 1-based page index.
    /// </summary>
    public int PageIndex { get; }

    /// <summary>
    /// Gets the page size.
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    /// Gets the total number of items across all pages.
    /// </summary>
    public int TotalCount { get; }

    /// <summary>
    /// Gets the items on this page.
    /// </summary>
    public IReadOnlyList<T> Data { get; }
}