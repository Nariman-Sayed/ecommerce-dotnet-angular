using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Ecom.API;
using Ecom.Core.DTO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace Ecom.IntegrationTests;

public class AuthorizationTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly string _imagesRoot;
    private readonly WebApplicationFactory<Program> _factory;

    public AuthorizationTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _imagesRoot = Path.Combine(Path.GetTempPath(), "ecom-auth-images", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_imagesRoot);

        using (var schema = new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options))
        {
            schema.Database.EnsureCreated();
        }

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                RemoveAll<DbContextOptions<AppDbContext>>(services);
                services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));

                RemoveAll<IFileProvider>(services);
                services.AddSingleton<IFileProvider>(new PhysicalFileProvider(_imagesRoot));

                services.AddTestAuthentication();
            });
        });
    }

    public void Dispose()
    {
        _factory.Dispose();
        _connection.Dispose();
        if (Directory.Exists(_imagesRoot))
            Directory.Delete(_imagesRoot, recursive: true);
    }

    [Fact]
    public async Task GetProducts_Anonymous_ReturnsOk()
    {
        using var client = CreateClient(null);
        var response = await client.GetAsync("/api/products");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetCategories_Anonymous_ReturnsOk()
    {
        using var client = CreateClient(null);
        var response = await client.GetAsync("/api/categories");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateCategory_WithoutToken_Returns401()
    {
        using var client = CreateClient(null);
        var response = await client.PostAsJsonAsync("/api/categories", new CategoryDTO("X", "Y"));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateCategory_WrongRole_Returns403()
    {
        using var client = CreateClient("customer");
        var response = await client.PostAsJsonAsync("/api/categories", new CategoryDTO("X", "Y"));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateCategory_Admin_ReturnsCreated()
    {
        using var client = CreateClient(AuthPolicies.AdminRole);
        var response = await client.PostAsJsonAsync("/api/categories", new CategoryDTO("AuthCat", "Desc"));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCategory_WithoutToken_Returns401()
    {
        using var client = CreateClient(null);
        var response = await client.PutAsJsonAsync("/api/categories/1", new UpdateCategoryDTO("X", "Y"));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCategory_WrongRole_Returns403()
    {
        using var client = CreateClient("customer");
        var response = await client.PutAsJsonAsync("/api/categories/1", new UpdateCategoryDTO("X", "Y"));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCategory_WithoutToken_Returns401()
    {
        using var client = CreateClient(null);
        var response = await client.DeleteAsync("/api/categories/1");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCategory_WrongRole_Returns403()
    {
        using var client = CreateClient("customer");
        var response = await client.DeleteAsync("/api/categories/1");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_WithoutToken_Returns401()
    {
        using var client = CreateClient(null);
        using var form = CreateProductForm("P", "D", 1, 1, 1, ("a.png", new byte[] { 1 }));
        var response = await client.PostAsync("/api/products", form);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_WrongRole_Returns403()
    {
        using var client = CreateClient("customer");
        using var form = CreateProductForm("P", "D", 1, 1, 1, ("a.png", new byte[] { 1 }));
        var response = await client.PostAsync("/api/products", form);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_Admin_ReturnsCreated()
    {
        using var adminClient = CreateClient(AuthPolicies.AdminRole);
        await adminClient.PostAsJsonAsync("/api/categories", new CategoryDTO("AuthCat2", "Desc"));
        var categories = await adminClient.GetFromJsonAsync<List<CategoryResponse>>("/api/categories");
        var categoryId = categories!.Single(c => c.Name == "AuthCat2").Id;

        using var client = CreateClient(AuthPolicies.AdminRole);
        using var form = CreateProductForm("Prod", "Desc", 10, 9, categoryId, ("a.png", new byte[] { 1 }));
        var response = await client.PostAsync("/api/products", form);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProduct_WithoutToken_Returns401()
    {
        using var client = CreateClient(null);
        using var form = CreateProductForm("P", "D", 1, 1, 1, ("a.png", new byte[] { 1 }));
        var response = await client.PutAsync("/api/products/1", form);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProduct_WrongRole_Returns403()
    {
        using var client = CreateClient("customer");
        using var form = CreateProductForm("P", "D", 1, 1, 1, ("a.png", new byte[] { 1 }));
        var response = await client.PutAsync("/api/products/1", form);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteProduct_WithoutToken_Returns401()
    {
        using var client = CreateClient(null);
        var response = await client.DeleteAsync("/api/products/1");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteProduct_WrongRole_Returns403()
    {
        using var client = CreateClient("customer");
        var response = await client.DeleteAsync("/api/products/1");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteProduct_Admin_ReturnsNoContent_WhenExists()
    {
        using var adminClient = CreateClient(AuthPolicies.AdminRole);
        await adminClient.PostAsJsonAsync("/api/categories", new CategoryDTO("AuthCat3", "Desc"));
        var categories = await adminClient.GetFromJsonAsync<List<CategoryResponse>>("/api/categories");
        var categoryId = categories!.Single(c => c.Name == "AuthCat3").Id;

        using var client = CreateClient(AuthPolicies.AdminRole);
        using var form = CreateProductForm("ProdDel", "Desc", 10, 9, categoryId, ("a.png", new byte[] { 1 }));
        var createResponse = await client.PostAsync("/api/products", form);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var id = ReadCreatedId(createResponse);

        var deleteResponse = await client.DeleteAsync($"/api/products/{id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task UpdateCategory_Admin_ReturnsOk()
    {
        using var adminClient = CreateClient(AuthPolicies.AdminRole);
        await adminClient.PostAsJsonAsync("/api/categories", new CategoryDTO("ToUpdate", "Desc"));
        var categories = await adminClient.GetFromJsonAsync<List<CategoryResponse>>("/api/categories");
        var id = categories!.Single(c => c.Name == "ToUpdate").Id;

        var response = await adminClient.PutAsJsonAsync($"/api/categories/{id}", new UpdateCategoryDTO("Updated", "New desc"));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCategory_Admin_ReturnsNoContent()
    {
        using var adminClient = CreateClient(AuthPolicies.AdminRole);
        await adminClient.PostAsJsonAsync("/api/categories", new CategoryDTO("ToDelete", "Desc"));
        var categories = await adminClient.GetFromJsonAsync<List<CategoryResponse>>("/api/categories");
        var id = categories!.Single(c => c.Name == "ToDelete").Id;

        var response = await adminClient.DeleteAsync($"/api/categories/{id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProduct_Admin_ReturnsOk()
    {
        using var adminClient = CreateClient(AuthPolicies.AdminRole);
        await adminClient.PostAsJsonAsync("/api/categories", new CategoryDTO("AuthCat4", "Desc"));
        var categories = await adminClient.GetFromJsonAsync<List<CategoryResponse>>("/api/categories");
        var categoryId = categories!.Single(c => c.Name == "AuthCat4").Id;

        using var client = CreateClient(AuthPolicies.AdminRole);
        using var form = CreateProductForm("ProdUpd", "Desc", 10, 9, categoryId, ("a.png", new byte[] { 1 }));
        var createResponse = await client.PostAsync("/api/products", form);
        var id = ReadCreatedId(createResponse);

        using var updateForm = CreateProductForm("ProdUpd2", "Desc2", 11, 10, categoryId, ("b.png", new byte[] { 2 }));
        var response = await client.PutAsync($"/api/products/{id}", updateForm);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private HttpClient CreateClient(string? role)
    {
        var client = _factory.CreateClient();
        if (role is not null)
            client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeaderName, role);

        return client;
    }

    private static int ReadCreatedId(HttpResponseMessage response)
    {
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
}
