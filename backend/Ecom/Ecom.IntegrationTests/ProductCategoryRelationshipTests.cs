using Microsoft.Data.Sqlite;

namespace Ecom.IntegrationTests;

public class ProductCategoryRelationshipTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _context;

    public ProductCategoryRelationshipTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task AddingProduct_WithValidCategory_PersistsAndLinksCategory()
    {
        var category = new Category { Name = "Electronics", Description = "Devices" };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var product = new Product
        {
            Name = "Laptop",
            Description = "Test laptop",
            Price = 999,
            CategoryId = category.Id
        };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var savedProduct = await _context.Products
            .Include(p => p.Category)
            .FirstAsync(p => p.Id == product.Id);

        Assert.Equal("Electronics", savedProduct.Category.Name);
    }

    [Fact]
    public async Task DeletingProduct_CascadesDeleteToPhotos()
    {
        var category = new Category { Name = "Books", Description = "Reading" };
        var product = new Product { Name = "Novel", Description = "Test", Price = 50, Category = category };
        product.Photos.Add(new Photo { ImageName = "cover.jpg" });
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        var remainingPhotos = await _context.Photos.CountAsync();
        Assert.Equal(0, remainingPhotos);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}