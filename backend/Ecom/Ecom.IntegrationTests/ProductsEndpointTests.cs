using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Ecom.Core.DTO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace Ecom.IntegrationTests;

public class ProductsEndpointTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly HttpClient _client;
    private readonly string _imagesRoot;

    public ProductsEndpointTests(WebApplicationFactory<Program> factory)
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _imagesRoot = Path.Combine(Path.GetTempPath(), "ecom-product-images", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_imagesRoot);

        using (var schema = new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options))
        {
            schema.Database.EnsureCreated();
        }

        var configuredFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                RemoveAll<DbContextOptions<AppDbContext>>(services);
                services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));

                RemoveAll<IFileProvider>(services);
                services.AddSingleton<IFileProvider>(new PhysicalFileProvider(_imagesRoot));
            });
        });

        _client = configuredFactory.CreateClient();
    }

    [Fact]
    public async Task Add_WithImages_StoresProductAndImages()
    {
        var category = await CreateCategoryAsync("Phones");

        var id = await AddProductAsync("Pixel", "Phone", 900, 800, category.Id, ("front.png", new byte[] { 1, 2, 3 }));

        var product = await GetProductAsync(id);

        Assert.Equal("Pixel", product.Name);
        Assert.Equal("Phone", product.Description);
        Assert.Equal(900, product.OldPrice);
        Assert.Equal(800, product.NewPrice);
        Assert.Equal("Phones", product.CategoryName);

        var photo = Assert.Single(product.Photos);
        Assert.Equal("/Images/Pixel/front.png", photo.ImageName);
        Assert.True(File.Exists(Path.Combine(_imagesRoot, "Images", "Pixel", "front.png")));
    }

    [Fact]
    public async Task GetAll_ReturnsStoredProducts()
    {
        var category = await CreateCategoryAsync("Books");
        await AddProductAsync("Novel", "Fiction", 30, 20, category.Id, ("cover.png", new byte[] { 1 }));

        var products = await _client.GetFromJsonAsync<List<ProductResponse>>("/api/products");

        var product = Assert.Single(products!);
        Assert.Equal("Novel", product.Name);
        Assert.Equal("Books", product.CategoryName);
    }

    [Fact]
    public async Task GetById_ExistingProduct_ReturnsProduct()
    {
        var category = await CreateCategoryAsync("Games");
        var id = await AddProductAsync("Console", "Gaming", 500, 450, category.Id, ("box.png", new byte[] { 1 }));

        var product = await GetProductAsync(id);

        Assert.Equal("Console", product.Name);
        Assert.Equal("Games", product.CategoryName);
        Assert.Single(product.Photos);
    }

    [Fact]
    public async Task Update_ChangesValuesAndReplacesImages()
    {
        var category = await CreateCategoryAsync("Toys");
        var id = await AddProductAsync("Lego", "Blocks", 100, 90, category.Id, ("old.png", new byte[] { 1 }));
        var oldImage = Path.Combine(_imagesRoot, "Images", "Lego", "old.png");
        Assert.True(File.Exists(oldImage));

        using var form = CreateProductForm("Lego Deluxe", "Big blocks", 120, 110, category.Id, ("new.png", new byte[] { 2 }));
        var response = await _client.PutAsync($"/api/products/{id}", form);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updated = await GetProductAsync(id);
        Assert.Equal("Lego Deluxe", updated.Name);
        Assert.Equal("Big blocks", updated.Description);
        Assert.Equal(120, updated.OldPrice);
        Assert.Equal(110, updated.NewPrice);

        var photo = Assert.Single(updated.Photos);
        Assert.Equal("/Images/Lego Deluxe/new.png", photo.ImageName);
        Assert.False(File.Exists(oldImage));
        Assert.True(File.Exists(Path.Combine(_imagesRoot, "Images", "Lego Deluxe", "new.png")));
    }

    [Fact]
    public async Task Delete_RemovesProductAndImages()
    {
        var category = await CreateCategoryAsync("Music");
        var id = await AddProductAsync("Vinyl", "Record", 40, 35, category.Id, ("disc.png", new byte[] { 1 }));
        var image = Path.Combine(_imagesRoot, "Images", "Vinyl", "disc.png");
        Assert.True(File.Exists(image));

        var response = await _client.DeleteAsync($"/api/products/{id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(File.Exists(image));

        var getResponse = await _client.GetAsync($"/api/products/{id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task GetById_NonExistingProduct_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/products/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("999999", body);
    }

    [Fact]
    public async Task Update_NonExistingProduct_ReturnsNotFound()
    {
        using var form = CreateProductForm("Ghost", "None", 1, 1, 1, ("ghost.png", new byte[] { 1 }));

        var response = await _client.PutAsync("/api/products/999999", form);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_NonExistingProduct_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync("/api/products/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    public void Dispose()
    {
        _client.Dispose();
        _connection.Dispose();

        if (Directory.Exists(_imagesRoot))
            Directory.Delete(_imagesRoot, recursive: true);
    }

    private async Task<CategoryResponse> CreateCategoryAsync(string name)
    {
        var response = await _client.PostAsJsonAsync("/api/categories", new CategoryDTO(name, "Description"));
        response.EnsureSuccessStatusCode();

        var categories = await _client.GetFromJsonAsync<List<CategoryResponse>>("/api/categories");
        return categories!.Single(c => c.Name == name);
    }

    private async Task<ProductResponse> GetProductAsync(int id)
    {
        var product = await _client.GetFromJsonAsync<ProductResponse>($"/api/products/{id}");
        return Assert.IsType<ProductResponse>(product);
    }

    private async Task<int> AddProductAsync(
        string name,
        string description,
        decimal oldPrice,
        decimal newPrice,
        int categoryId,
        params (string FileName, byte[] Content)[] files)
    {
        using var form = CreateProductForm(name, description, oldPrice, newPrice, categoryId, files);

        var response = await _client.PostAsync("/api/products", form);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var segments = response.Headers.Location!.ToString().Split('/', StringSplitOptions.RemoveEmptyEntries);
        return int.Parse(segments[^1], CultureInfo.InvariantCulture);
    }

    private static MultipartFormDataContent CreateProductForm(
        string name,
        string description,
        decimal oldPrice,
        decimal newPrice,
        int categoryId,
        params (string FileName, byte[] Content)[] files)
    {
        var form = new MultipartFormDataContent
        {
            { new StringContent(name), "Name" },
            { new StringContent(description), "Description" },
            { new StringContent(oldPrice.ToString(CultureInfo.InvariantCulture)), "OldPrice" },
            { new StringContent(newPrice.ToString(CultureInfo.InvariantCulture)), "NewPrice" },
            { new StringContent(categoryId.ToString(CultureInfo.InvariantCulture)), "CategoryId" }
        };

        foreach (var file in files)
        {
            var content = new ByteArrayContent(file.Content);
            content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            form.Add(content, "Photo", file.FileName);
        }

        return form;
    }

    private static void RemoveAll<T>(IServiceCollection services)
    {
        foreach (var descriptor in services.Where(d => d.ServiceType == typeof(T)).ToList())
            services.Remove(descriptor);
    }

    private sealed record CategoryResponse(int Id, string Name, string Description);

    private sealed record ProductResponse(
        int Id,
        string Name,
        string Description,
        decimal OldPrice,
        decimal NewPrice,
        string CategoryName,
        List<PhotoResponse> Photos);

    private sealed record PhotoResponse(string ImageName, int ProductId);
}
