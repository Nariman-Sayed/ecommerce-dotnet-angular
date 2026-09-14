using System.Reflection;
using NetArchTest.Rules;

namespace Ecom.IntegrationTests;

public class ArchitectureTests
{
    private static readonly Assembly CoreAssembly = typeof(Category).Assembly;

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
}