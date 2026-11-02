using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace Ecom.IntegrationTests;

public class ProductQueryEndpointTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly HttpClient _client;
    private readonly int _booksCategoryId;

    public ProductQueryEndpointTests(WebApplicationFactory<Program> factory)
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        using (var context = new SqliteAppDbContext(
            new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options))
        {
            context.Database.EnsureCreated();

            var electronics = new Category("Electronics", "Devices and gadgets");
            var books = new Category("Books", "Reading material");
            context.Categories.AddRange(electronics, books);
            context.SaveChanges();

            _booksCategoryId = books.Id;

            context.Products.AddRange(
                new Product("Laptop", "Portable computer", electronics.Id, 1199, 999),
                new Product("Phone", "Smartphone device", electronics.Id, 899, 749),
                new Product("Headphones", "Wireless audio", electronics.Id, 199, 149),
                new Product("Camera", "Digital camera", electronics.Id, 699, 599),
                new Product("Novel", "Paperback fiction", books.Id, 25, 15),
                new Product("Cookbook", "Recipes and tips", books.Id, 30, 22),
                new Product("Atlas", "World maps", books.Id, 35, 28),
                new Product("Poetry", "Collected poems", books.Id, 18, 10));

            context.SaveChanges();
        }

        var configuredFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                RemoveAll<DbContextOptions<AppDbContext>>(services);
                services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));
                RemoveAll<AppDbContext>(services);
                services.AddScoped<AppDbContext>(provider =>
                    new SqliteAppDbContext(provider.GetRequiredService<DbContextOptions<AppDbContext>>()));
            });
        });

        _client = configuredFactory.CreateClient();
    }

    [Fact]
    public async Task GetAll_SortPriceAscending_ReturnsPricesInAscendingOrder()
    {
        var page = await GetPageAsync("/api/products?sort=priceAsc&pageSize=6");

        Assert.Equal(8, page.TotalCount);
        Assert.Equal(
            new[] { "Poetry", "Novel", "Cookbook", "Atlas", "Headphones", "Camera" },
            page.Data.Select(p => p.Name));
        Assert.Equal(
            page.Data.Select(p => p.NewPrice).OrderBy(price => price),
            page.Data.Select(p => p.NewPrice));
    }

    [Fact]
    public async Task GetAll_SortPriceDescending_ReturnsPricesInDescendingOrder()
    {
        var page = await GetPageAsync("/api/products?sort=priceDesc&pageSize=6");

        Assert.Equal(8, page.TotalCount);
        Assert.Equal(
            new[] { "Laptop", "Phone", "Camera", "Headphones", "Atlas", "Cookbook" },
            page.Data.Select(p => p.Name));
        Assert.Equal(
            page.Data.Select(p => p.NewPrice).OrderByDescending(price => price),
            page.Data.Select(p => p.NewPrice));
    }

    [Fact]
    public async Task GetAll_FilterByCategory_ReturnsOnlyThatCategory()
    {
        var page = await GetPageAsync($"/api/products?categoryId={_booksCategoryId}&pageSize=6");

        Assert.Equal(4, page.TotalCount);
        Assert.Equal(
            new[] { "Novel", "Cookbook", "Atlas", "Poetry" },
            page.Data.Select(p => p.Name));
        Assert.All(page.Data, product => Assert.Equal("Books", product.CategoryName));
    }

    [Theory]
    [InlineData("LAPTOP", "Laptop")]
    [InlineData("portable", "Laptop")]
    [InlineData("fiction", "Novel")]
    [InlineData("camera", "Camera")]
    public async Task GetAll_Search_MatchesNameOrDescriptionCaseInsensitively(
        string search,
        string expectedName)
    {
        var page = await GetPageAsync($"/api/products?search={Uri.EscapeDataString(search)}&pageSize=6");

        var product = Assert.Single(page.Data);
        Assert.Equal(expectedName, product.Name);
        Assert.Equal(1, page.TotalCount);
    }

    [Fact]
    public async Task GetAll_Search_MultipleWords_RequireEveryWordToMatch()
    {
        var match = await GetPageAsync("/api/products?search=wireless%20audio&pageSize=6");
        var noMatch = await GetPageAsync("/api/products?search=wireless%20recipes&pageSize=6");

        var product = Assert.Single(match.Data);
        Assert.Equal("Headphones", product.Name);
        Assert.Equal(1, match.TotalCount);

        Assert.Empty(noMatch.Data);
        Assert.Equal(0, noMatch.TotalCount);
    }

    [Fact]
    public async Task GetAll_Count_RemainsTheTotalAcrossMultiplePages()
    {
        var page1 = await GetPageAsync("/api/products?pageIndex=1&pageSize=3");
        var page2 = await GetPageAsync("/api/products?pageIndex=2&pageSize=3");
        var page3 = await GetPageAsync("/api/products?pageIndex=3&pageSize=3");

        Assert.All(new[] { page1, page2, page3 }, page => Assert.Equal(8, page.TotalCount));

        Assert.Equal(new[] { "Laptop", "Phone", "Headphones" }, page1.Data.Select(p => p.Name));
        Assert.Equal(new[] { "Camera", "Novel", "Cookbook" }, page2.Data.Select(p => p.Name));
        Assert.Equal(new[] { "Atlas", "Poetry" }, page3.Data.Select(p => p.Name));

        Assert.Equal(3, page1.Data.Count);
        Assert.Equal(3, page2.Data.Count);
        Assert.Equal(2, page3.Data.Count);

        var namesAcrossPages = page1.Data.Concat(page2.Data).Concat(page3.Data)
            .Select(p => p.Name)
            .ToList();
        Assert.Equal(8, namesAcrossPages.Distinct().Count());
    }

    [Fact]
    public async Task GetAll_PageSizeAboveTheCap_IsClampedToSix()
    {
        var page = await GetPageAsync("/api/products?pageSize=100");

        Assert.Equal(6, page.PageSize);
        Assert.Equal(6, page.Data.Count);
        Assert.Equal(8, page.TotalCount);
    }

    public void Dispose()
    {
        _client.Dispose();
        _connection.Dispose();
    }

    private async Task<PaginationResponse<ProductResponse>> GetPageAsync(string url)
    {
        var page = await _client.GetFromJsonAsync<PaginationResponse<ProductResponse>>(url);
        return Assert.IsType<PaginationResponse<ProductResponse>>(page);
    }

    private static void RemoveAll<T>(IServiceCollection services)
    {
        foreach (var descriptor in services.Where(d => d.ServiceType == typeof(T)).ToList())
            services.Remove(descriptor);
    }

    // SQLite maps decimal to TEXT and refuses to translate ORDER BY over a
    // decimal column. SQL Server, the production provider, orders decimals
    // natively, so this mapping exists only to let the test provider exercise
    // the same LINQ query.
    private sealed class SqliteAppDbContext : AppDbContext
    {
        public SqliteAppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>().Property(p => p.OldPrice).HasConversion<double>();
            modelBuilder.Entity<Product>().Property(p => p.NewPrice).HasConversion<double>();
        }
    }

    private sealed record ProductResponse(
        string Name,
        string Description,
        decimal OldPrice,
        decimal NewPrice,
        string CategoryName,
        List<PhotoResponse> Photos);

    private sealed record PhotoResponse(string ImageName, int ProductId);

    private sealed record PaginationResponse<T>(
        int PageIndex,
        int PageSize,
        int TotalCount,
        List<T> Data);
}
