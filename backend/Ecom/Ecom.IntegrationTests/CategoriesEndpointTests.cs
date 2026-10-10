using System.Net;
using System.Net.Http.Json;
using Ecom.Core.DTO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace Ecom.IntegrationTests;

public class CategoriesEndpointTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly HttpClient _client;

    public CategoriesEndpointTests(WebApplicationFactory<Program> factory)
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

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
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor is not null)
                    services.Remove(descriptor);

                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlite(_connection));
            });
        });

        _client = configuredFactory.CreateClient();
    }

    [Fact]
    public async Task Add_Then_GetAll_ReturnsAddedCategory()
    {
        var response = await _client.PostAsJsonAsync("/api/categories", new CategoryDTO("Electronics", "Devices"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var categories = await _client.GetFromJsonAsync<List<CategoryResponse>>("/api/categories");

        Assert.NotNull(categories);
        Assert.Contains(categories!, c => c.Name == "Electronics" && c.Description == "Devices");
    }

    [Fact]
    public async Task GetById_ExistingCategory_ReturnsCategory()
    {
        await _client.PostAsJsonAsync("/api/categories", new CategoryDTO("Books", "Reading"));
        var created = await GetSingleCategoryAsync();

        var category = await _client.GetFromJsonAsync<CategoryResponse>($"/api/categories/{created.Id}");

        Assert.NotNull(category);
        Assert.Equal("Books", category!.Name);
        Assert.Equal("Reading", category.Description);
    }

    [Fact]
    public async Task Update_ChangesNameAndDescription()
    {
        await _client.PostAsJsonAsync("/api/categories", new CategoryDTO("Old name", "Old description"));
        var created = await GetSingleCategoryAsync();

        var response = await _client.PutAsJsonAsync(
            $"/api/categories/{created.Id}",
            new UpdateCategoryDTO("New name", "New description"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updated = await _client.GetFromJsonAsync<CategoryResponse>($"/api/categories/{created.Id}");

        Assert.NotNull(updated);
        Assert.Equal("New name", updated!.Name);
        Assert.Equal("New description", updated.Description);
    }

    [Fact]
    public async Task Delete_RemovesCategory()
    {
        await _client.PostAsJsonAsync("/api/categories", new CategoryDTO("Temporary", "To delete"));
        var created = await GetSingleCategoryAsync();

        var response = await _client.DeleteAsync($"/api/categories/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var getResponse = await _client.GetAsync($"/api/categories/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task GetById_NonExistingCategory_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/categories/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("999999", body);
    }

    [Fact]
    public async Task Update_NonExistingCategory_ReturnsNotFound()
    {
        var response = await _client.PutAsJsonAsync(
            "/api/categories/999999",
            new UpdateCategoryDTO("Name", "Description"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_NonExistingCategory_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync("/api/categories/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<CategoryResponse> GetSingleCategoryAsync()
    {
        var categories = await _client.GetFromJsonAsync<List<CategoryResponse>>("/api/categories");
        return Assert.Single(categories!);
    }

    public void Dispose()
    {
        _client.Dispose();
        _connection.Dispose();
    }

    private sealed record CategoryResponse(int Id, string Name, string Description);
}
