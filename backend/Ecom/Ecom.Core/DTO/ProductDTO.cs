namespace Ecom.Core.DTO;

public record ProductDTO
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public decimal OldPrice { get; set; }
    public decimal NewPrice { get; set; }
    public List<PhotoDTO> Photos { get; set; } = new();
    public required string CategoryName { get; set; }
}

public record PhotoDTO
{
    public required string ImageName { get; set; }
    public int ProductId { get; set; }
}
