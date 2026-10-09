namespace Ecom.Core.Entities.Product;

public class Category : BaseEntity<int>
{
    private readonly List<Product> _products = new();

    private Category()
    {
        // EF Core materialization only.
    }

    public Category(string name, string description)
    {
        UpdateDetails(name, description);
    }

    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public IReadOnlyCollection<Product> Products => _products;

    public Category UpdateDetails(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name is required.", nameof(name));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Category description is required.", nameof(description));

        Name = name;
        Description = description;
        return this;
    }

    public void AddProduct(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);
        _products.Add(product);
    }

    public void RemoveProduct(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);
        _products.Remove(product);
    }
}