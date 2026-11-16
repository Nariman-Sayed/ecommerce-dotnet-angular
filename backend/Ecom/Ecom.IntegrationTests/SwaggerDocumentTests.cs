using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;

namespace Ecom.IntegrationTests;

public class SwaggerDocumentTests
{
    [Fact]
    public void GetSwagger_RequiresAuthorizationOnlyForAuthorizedOperations()
    {
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.UseEnvironment("Testing"));
        using var scope = factory.Services.CreateScope();

        var provider = scope.ServiceProvider.GetRequiredService<ISwaggerProvider>();
        var document = provider.GetSwagger("v1");

        AssertNoSecurity(document, "/api/products", OperationType.Get);
        AssertNoSecurity(document, "/api/categories", OperationType.Get);
        AssertNoSecurity(document, "/api/products/{id}", OperationType.Get);
        AssertNoSecurity(document, "/api/categories/{id}", OperationType.Get);

        AssertSecurity(document, "/api/products", OperationType.Post);
        AssertSecurity(document, "/api/categories", OperationType.Post);
        AssertSecurity(document, "/api/products/{id}", OperationType.Put);
        AssertSecurity(document, "/api/categories/{id}", OperationType.Put);
        AssertSecurity(document, "/api/products/{id}", OperationType.Delete);
        AssertSecurity(document, "/api/categories/{id}", OperationType.Delete);
    }

    private static void AssertNoSecurity(
        OpenApiDocument document,
        string path,
        OperationType operationType)
    {
        var operation = GetOperation(document, path, operationType);
        Assert.True(
            operation.Security is null || operation.Security.Count == 0,
            $"{operationType} {path} should not require authorization.");
    }

    private static void AssertSecurity(
        OpenApiDocument document,
        string path,
        OperationType operationType)
    {
        var operation = GetOperation(document, path, operationType);
        var requirement = Assert.Single(operation.Security!);
        var scheme = Assert.Single(requirement.Keys);

        Assert.Equal("oauth2", scheme.Reference.Id);
    }

    private static OpenApiOperation GetOperation(
        OpenApiDocument document,
        string path,
        OperationType operationType)
    {
        var pathItem = document.Paths
            .Single(entry => string.Equals(entry.Key, path, StringComparison.OrdinalIgnoreCase))
            .Value;

        return pathItem.Operations[operationType];
    }
}
