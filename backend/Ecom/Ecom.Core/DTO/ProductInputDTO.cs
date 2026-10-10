namespace Ecom.Core.DTO;

public record AddProductDTO
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public decimal OldPrice { get; set; }
    public decimal NewPrice { get; set; }
    public int CategoryId { get; set; }
}

public record UpdateProductDTO : AddProductDTO;
