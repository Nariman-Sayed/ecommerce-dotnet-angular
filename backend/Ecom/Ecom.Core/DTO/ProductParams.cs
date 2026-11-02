namespace Ecom.Core.DTO;

/// <summary>
/// Query parameters for the product list endpoint.
/// </summary>
public class ProductParams
{
    private const int MaxPageSize = 6;

    private int _pageSize = 1;
    private int _pageIndex = 1;

    /// <summary>
    /// Optional sort key: <c>priceAsc</c> or <c>priceDesc</c>. Any other value falls back to id order.
    /// </summary>
    public string? Sort { get; set; }

    /// <summary>
    /// Optional category filter.
    /// </summary>
    public int? CategoryId { get; set; }

    /// <summary>
    /// Optional whitespace-separated keyword search matched against name or description.
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// 1-based page index. Values less than or equal to zero default to 1.
    /// </summary>
    public int PageIndex
    {
        get => _pageIndex;
        set => _pageIndex = value <= 0 ? 1 : value;
    }

    /// <summary>
    /// Page size, clamped to the range 1..6. Values less than or equal to zero default to 1.
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value <= 0 ? 1 : (value > MaxPageSize ? MaxPageSize : value);
    }
}