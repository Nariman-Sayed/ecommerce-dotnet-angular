using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace Ecom.IntegrationTests;

public class ErrorHandlingTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly HttpClient _client;

    public ErrorHandlingTests(WebApplicationFactory<Program> factory)
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
    public async Task NotFoundRoute_ReturnsFormattedErrorResponse()
    {
        var response = await _client.GetAsync("/api/categories/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("999999", body);
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}
