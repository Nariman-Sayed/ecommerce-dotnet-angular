using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Ecom.API;
using Ecom.Core.DTO;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Ecom.IntegrationTests;

public class JwtBearerIntegrationTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly string _imagesRoot;
    private readonly TestJwtTokenFactory _tokens = new();
    private readonly WebApplicationFactory<Program> _factory;

    public JwtBearerIntegrationTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _imagesRoot = Path.Combine(Path.GetTempPath(), "ecom-jwt-images", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_imagesRoot);

        using (var schema = new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options))
        {
            schema.Database.EnsureCreated();
        }

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureTestServices(services =>
            {
                RemoveAll<DbContextOptions<AppDbContext>>(services);
                services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));

                RemoveAll<IFileProvider>(services);
                services.AddSingleton<IFileProvider>(new PhysicalFileProvider(_imagesRoot));

                services.Configure<JwtBearerOptions>(
                    JwtBearerDefaults.AuthenticationScheme,
                    options =>
                    {
                        options.Authority = string.Empty;
                        options.MetadataAddress = string.Empty;
                        options.Configuration = new OpenIdConnectConfiguration
                        {
                            Issuer = TestJwtTokenFactory.Issuer,
                            SigningKeys = { _tokens.SigningKey }
                        };
                        options.TokenValidationParameters.IssuerSigningKey = _tokens.SigningKey;
                        options.TokenValidationParameters.ValidIssuer = TestJwtTokenFactory.Issuer;
                        options.TokenValidationParameters.ValidAudience = TestJwtTokenFactory.Audience;
                    });
            });
        });
    }

    [Fact]
    public async Task CreateCategory_WithRealAdminToken_ReturnsCreated()
    {
        using var client = CreateClient(_tokens.CreateToken(AuthPolicies.AdminRole));

        var response = await client.PostAsJsonAsync("/api/categories", new CategoryDTO("RealAdmin", "Desc"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateCategory_WithRealCustomerToken_ReturnsForbidden()
    {
        using var client = CreateClient(_tokens.CreateToken("customer"));

        var response = await client.PostAsJsonAsync("/api/categories", new CategoryDTO("RealCustomer", "Desc"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateCategory_WithWrongAudience_ReturnsUnauthorized()
    {
        using var client = CreateClient(_tokens.CreateToken(AuthPolicies.AdminRole, audience: "other-api"));

        var response = await client.PostAsJsonAsync("/api/categories", new CategoryDTO("WrongAudience", "Desc"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateCategory_WithoutToken_ReturnsUnauthorized()
    {
        using var client = CreateClient(null);

        var response = await client.PostAsJsonAsync("/api/categories", new CategoryDTO("NoToken", "Desc"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateCategory_WithExpiredToken_ReturnsUnauthorized()
    {
        using var client = CreateClient(
            _tokens.CreateToken(AuthPolicies.AdminRole, expires: DateTime.UtcNow.AddMinutes(-10)));

        var response = await client.PostAsJsonAsync("/api/categories", new CategoryDTO("Expired", "Desc"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateCategory_WithDifferentSigningKey_ReturnsUnauthorized()
    {
        using var client = CreateClient(
            _tokens.CreateToken(
                AuthPolicies.AdminRole,
                signingKey: TestJwtTokenFactory.CreateDifferentSigningKey()));

        var response = await client.PostAsJsonAsync("/api/categories", new CategoryDTO("WrongKey", "Desc"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_WithRealAdminToken_ReturnsCreated()
    {
        using var adminClient = CreateClient(_tokens.CreateToken(AuthPolicies.AdminRole));
        var categoryResponse = await adminClient.PostAsJsonAsync(
            "/api/categories",
            new CategoryDTO("RealProductCategory", "Desc"));
        Assert.Equal(HttpStatusCode.Created, categoryResponse.StatusCode);

        var categories = await adminClient.GetFromJsonAsync<List<CategoryResponse>>("/api/categories");
        var categoryId = categories!.Single(category => category.Name == "RealProductCategory").Id;

        using var form = CreateProductForm("RealProduct", "Desc", 10, 9, categoryId);
        var response = await adminClient.PostAsync("/api/products", form);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
    }

    public void Dispose()
    {
        _factory.Dispose();
        _connection.Dispose();

        if (Directory.Exists(_imagesRoot))
            Directory.Delete(_imagesRoot, recursive: true);
    }

    private HttpClient CreateClient(string? token)
    {
        var client = _factory.CreateClient();
        if (token is not null)
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    private static MultipartFormDataContent CreateProductForm(
        string name,
        string description,
        decimal oldPrice,
        decimal newPrice,
        int categoryId)
    {
        var form = new MultipartFormDataContent
        {
            { new StringContent(name), "Name" },
            { new StringContent(description), "Description" },
            { new StringContent(oldPrice.ToString(CultureInfo.InvariantCulture)), "OldPrice" },
            { new StringContent(newPrice.ToString(CultureInfo.InvariantCulture)), "NewPrice" },
            { new StringContent(categoryId.ToString(CultureInfo.InvariantCulture)), "CategoryId" }
        };

        var content = new ByteArrayContent(new byte[] { 1 });
        content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        form.Add(content, "Photo", "photo.png");

        return form;
    }

    private static void RemoveAll<T>(IServiceCollection services)
    {
        foreach (var descriptor in services.Where(d => d.ServiceType == typeof(T)).ToList())
            services.Remove(descriptor);
    }

    private sealed record CategoryResponse(int Id, string Name, string Description);
}
