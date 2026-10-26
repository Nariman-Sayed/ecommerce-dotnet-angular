using Microsoft.AspNetCore.Http;

namespace Ecom.API.DTO;

/// <summary>
/// The payload for creating a product.
/// </summary>
public record AddProductRequest
{
    /// <summary>
    /// Gets or sets the display name of the product.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the description of the product.
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// Gets or sets the original price of the product before any discount.
    /// </summary>
    public decimal OldPrice { get; set; }

    /// <summary>
    /// Gets or sets the current selling price of the product.
    /// </summary>
    public decimal NewPrice { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the category the product belongs to.
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Gets or sets the uploaded image files for the product.
    /// </summary>
    public required IFormFileCollection Photo { get; set; }
}

/// <summary>
/// The payload for updating a product; inherits all product fields.
/// </summary>
public record UpdateProductRequest : AddProductRequest;
