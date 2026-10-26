using Ecom.Core.Entities.Product;

namespace Ecom.Core.DTO;

public record AddProductDTO
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public decimal OldPrice { get; set; }
    public decimal NewPrice { get; set; }
    public int CategoryId { get; set; }

    public Product ToProduct() => new(Name, Description, CategoryId, OldPrice, NewPrice);
}

public record UpdateProductDTO : AddProductDTO;
