using Microsoft.Data.Sqlite;

namespace Ecom.IntegrationTests;

public class ProductCategoryRelationshipTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _context;

    public ProductCategoryRelationshipTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:;Foreign Keys=True");
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
            NewPrice = 999,
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
        var product = new Product { Name = "Novel", Description = "Test", NewPrice = 50, Category = category };
        product.Photos.Add(new Photo { ImageName = "cover.jpg" });
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var productId = product.Id;

        var fetchedProduct = await _context.Products
            .Include(p => p.Photos)
            .FirstAsync(p => p.Id == productId);

        _context.Products.Remove(fetchedProduct);
        await _context.SaveChangesAsync();

        var remainingPhotosForProduct = await _context.Photos
            .Where(p => p.ProductId == productId)
            .CountAsync();

        Assert.Equal(0, remainingPhotosForProduct);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
