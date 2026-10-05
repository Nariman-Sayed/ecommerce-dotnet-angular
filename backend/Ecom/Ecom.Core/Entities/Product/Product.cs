namespace Ecom.Core.Entities.Product;

public class Product : BaseEntity<int>
{
    private Product()
    {
        // EF Core materialization only.
    }

    public Product(string name, string description, int categoryId, decimal oldPrice, decimal newPrice)
    {
        UpdateDetails(name, description);
        UpdatePricing(oldPrice, newPrice);
        CategoryId = categoryId;
    }

    private readonly List<Photo> _photos = new();

    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public decimal OldPrice { get; private set; }
    public decimal NewPrice { get; private set; }
    public IReadOnlyCollection<Photo> Photos => _photos;
    public int CategoryId { get; private set; }
    public Category Category { get; set; } = null!;

    public void UpdateDetails(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name is required.", nameof(name));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Product description is required.", nameof(description));

        Name = name;
        Description = description;
    }

    public void UpdatePricing(decimal oldPrice, decimal newPrice)
    {
        if (oldPrice < 0)
            throw new ArgumentException("Old price cannot be negative.", nameof(oldPrice));

        if (newPrice < 0)
            throw new ArgumentException("New price cannot be negative.", nameof(newPrice));

        OldPrice = oldPrice;
        NewPrice = newPrice;
    }

    public void AddPhoto(Photo photo)
    {
        ArgumentNullException.ThrowIfNull(photo);
        _photos.Add(photo);
    }

    public void RemovePhoto(Photo photo)
    {
        ArgumentNullException.ThrowIfNull(photo);
        _photos.Remove(photo);
    }
}