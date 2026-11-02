using System.Reflection;
using NetArchTest.Rules;

namespace Ecom.IntegrationTests;

public class ArchitectureTests
{
    private static readonly Assembly CoreAssembly = typeof(Category).Assembly;
    private static readonly Assembly ApplicationAssembly = Assembly.Load("Ecom.Application");

    [Fact]
    public void Core_Should_Not_HaveDependencyOn_EntityFrameworkCore()
    {
        var result = Types.InAssembly(CoreAssembly)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Core layer must not depend on EF Core. Violating types: " +
            $"{string.Join(", ", result.FailingTypeNames ?? Enumerable.Empty<string>())}");
    }

    [Fact]
    public void Core_Should_Not_HaveDependencyOn_AnyPersistenceLibrary()
    {
        var result = Types.InAssembly(CoreAssembly)
            .ShouldNot()
            .HaveDependencyOnAny("Microsoft.EntityFrameworkCore", "Microsoft.Data.SqlClient", "Microsoft.Data.Sqlite", "Dapper")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Core layer must stay persistence-agnostic. Violating types: " +
            $"{string.Join(", ", result.FailingTypeNames ?? Enumerable.Empty<string>())}");
    }

    [Fact]
    public void Core_Should_Not_HaveDependencyOn_OtherLayers()
    {
        var result = Types.InAssembly(CoreAssembly)
            .ShouldNot()
            .HaveDependencyOnAny("Ecom.infrastructure", "Ecom.Application", "Ecom.API")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Core layer must not depend on Infrastructure, Application, or API. Violating types: " +
            $"{string.Join(", ", result.FailingTypeNames ?? Enumerable.Empty<string>())}");
    }

    [Fact]
    public void Application_Should_Not_Reference_InfrastructureOrApi()
    {
        var referencedIncorrectly = ApplicationAssembly.GetReferencedAssemblies()
            .Select(a => a.Name)
            .Where(name => name == "Ecom.infrastructure" || name == "Ecom.API");

        Assert.Empty(referencedIncorrectly);
    }

    [Fact]
    public void Application_Should_Not_HaveDependencyOn_InfrastructureOrApi()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny("Ecom.infrastructure", "Ecom.API")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Application layer must not depend on Infrastructure or API. Violating types: " +
            $"{string.Join(", ", result.FailingTypeNames ?? Enumerable.Empty<string>())}");
    }

    // Application deliberately references Microsoft.EntityFrameworkCore so that
    // IApplicationDbContext can expose DbSet<T>, and ProductService can compose
    // IQueryable queries with Include/AsNoTracking. A blanket EF Core ban would
    // therefore fail by design, not because of a layering mistake. The rule that
    // matches this design is to keep the chosen persistence provider out of the
    // Application layer; only the infrastructure project may reference it.
    [Fact]
    public void Application_Should_Not_HaveDependencyOn_PersistenceProviders()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny("Microsoft.EntityFrameworkCore.SqlServer", "Microsoft.EntityFrameworkCore.Sqlite", "Microsoft.Data.SqlClient", "Microsoft.Data.Sqlite", "Dapper")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Application layer must not depend on a concrete persistence provider. Violating types: " +
            $"{string.Join(", ", result.FailingTypeNames ?? Enumerable.Empty<string>())}");
    }
}
