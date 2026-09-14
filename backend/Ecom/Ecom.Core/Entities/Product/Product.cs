namespace Ecom.Core.Entities.Product;

public class Product : BaseEntity<int>
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public decimal Price { get; set; }
    public List<Photo> Photos { get; set; } = new();
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
}