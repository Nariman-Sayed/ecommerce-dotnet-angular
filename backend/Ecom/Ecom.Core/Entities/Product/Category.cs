namespace Ecom.Core.Entities.Product;

public class Category : BaseEntity<int>
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public ICollection<Product> Products { get; set; } = new HashSet<Product>();
}