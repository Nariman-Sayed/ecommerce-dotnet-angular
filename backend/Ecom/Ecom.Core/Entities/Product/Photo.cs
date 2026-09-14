namespace Ecom.Core.Entities.Product;

public class Photo : BaseEntity<int>
{
    public required string ImageName { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
}