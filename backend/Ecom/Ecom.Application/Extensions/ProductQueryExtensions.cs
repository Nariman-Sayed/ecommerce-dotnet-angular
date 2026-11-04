using Ecom.Core.Entities.Product;

namespace Ecom.Application.Extensions;

/// <summary>
/// Query-building helpers for <see cref="Product"/> sequences.
/// </summary>
public static class ProductQueryExtensions
{
    /// <summary>
    /// Filters the query to products in the given category.
    /// </summary>
    /// <param name="query">The source query.</param>
    /// <param name="categoryId">The category to filter by, or <c>null</c> for no filter.</param>
    /// <returns>The filtered query.</returns>
    public static IQueryable<Product> ApplyFilter(this IQueryable<Product> query, int? categoryId)
    {
        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        return query;
    }

    /// <summary>
    /// Searches the query by requiring every whitespace-separated word to appear
    /// in the product name or description (case-insensitive).
    /// </summary>
    /// <param name="query">The source query.</param>
    /// <param name="search">The search text.</param>
    /// <returns>The filtered query.</returns>
    public static IQueryable<Product> ApplySearch(this IQueryable<Product> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }

        var words = search
            .Trim()
            .Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var word in words)
        {
            var term = word.ToLowerInvariant();
            query = query.Where(p =>
                p.Name.ToLower().Contains(term) ||
                p.Description.ToLower().Contains(term));
        }

        return query;
    }

    /// <summary>
    /// Sorts the query by price or, by default, by id for stable paging.
    /// </summary>
    /// <param name="query">The source query.</param>
    /// <param name="sort">The sort key.</param>
    /// <returns>The sorted query.</returns>
    public static IQueryable<Product> ApplySort(this IQueryable<Product> query, string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            return query.OrderBy(p => p.Id);
        }

        return sort.Trim().ToLowerInvariant() switch
        {
            "priceasc" => query.OrderBy(p => p.NewPrice).ThenBy(p => p.Id),
            "pricedesc" => query.OrderByDescending(p => p.NewPrice).ThenBy(p => p.Id),
            _ => query.OrderBy(p => p.Id)
        };
    }

    /// <summary>
    /// Applies skip/take paging to the query.
    /// </summary>
    /// <param name="query">The source query.</param>
    /// <param name="pageIndex">The 1-based page index.</param>
    /// <param name="pageSize">The page size.</param>
    /// <returns>The paged query.</returns>
    public static IQueryable<Product> ApplyPaging(this IQueryable<Product> query, int pageIndex, int pageSize)
    {
        return query.Skip(pageSize * (pageIndex - 1)).Take(pageSize);
    }
}