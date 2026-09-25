namespace Ecom.Core.Entities.Product;

public class Category : BaseEntity<int>
{
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
    public ICollection<Product> Products { get; set; } = new HashSet<Product>();

    public void UpdateDetails(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name is required.", nameof(name));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Category description is required.", nameof(description));

        Name = name;
        Description = description;
    }
}