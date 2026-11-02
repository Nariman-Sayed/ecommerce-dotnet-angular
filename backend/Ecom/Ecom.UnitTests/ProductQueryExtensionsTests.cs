using Ecom.Application.Extensions;
using Ecom.Core.Entities.Product;

namespace Ecom.UnitTests;

public class ProductQueryExtensionsTests
{
    [Fact]
    public void ApplySort_PriceAscending_OrdersByNewPriceThenId()
    {
        var products = new[]
        {
            CreateProduct(id: 2, name: "Tablet", description: "Slate device", price: 20),
            CreateProduct(id: 3, name: "Cable", description: "Charging cable", price: 10),
            CreateProduct(id: 1, name: "Phone", description: "Pocket device", price: 20)
        }.AsQueryable();

        var result = products.ApplySort("priceAsc").ToList();

        Assert.Equal(new[] { 3, 1, 2 }, result.Select(p => p.Id));
        Assert.Equal(new[] { 10m, 20m, 20m }, result.Select(p => p.NewPrice));
    }

    [Fact]
    public void ApplySort_PriceDescending_OrdersByNewPriceThenId()
    {
        var products = new[]
        {
            CreateProduct(id: 2, name: "Tablet", description: "Slate device", price: 20),
            CreateProduct(id: 3, name: "Cable", description: "Charging cable", price: 10),
            CreateProduct(id: 1, name: "Phone", description: "Pocket device", price: 20)
        }.AsQueryable();

        var result = products.ApplySort("priceDesc").ToList();

        Assert.Equal(new[] { 1, 2, 3 }, result.Select(p => p.Id));
        Assert.Equal(new[] { 20m, 20m, 10m }, result.Select(p => p.NewPrice));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("newest")]
    public void ApplySort_UnknownOrEmptyValue_FallsBackToIdOrder(string? sort)
    {
        var products = new[]
        {
            CreateProduct(id: 3, name: "Cable", description: "Charging cable", price: 10),
            CreateProduct(id: 1, name: "Phone", description: "Pocket device", price: 30),
            CreateProduct(id: 2, name: "Tablet", description: "Slate device", price: 20)
        }.AsQueryable();

        var result = products.ApplySort(sort).ToList();

        Assert.Equal(new[] { 1, 2, 3 }, result.Select(p => p.Id));
    }

    [Fact]
    public void ApplyFilter_WithCategoryId_ReturnsOnlyThatCategory()
    {
        var products = new[]
        {
            CreateProduct(id: 1, name: "Phone", description: "Pocket device", price: 20, categoryId: 10),
            CreateProduct(id: 2, name: "Tablet", description: "Slate device", price: 30, categoryId: 10),
            CreateProduct(id: 3, name: "Novel", description: "Paper fiction", price: 5, categoryId: 20)
        }.AsQueryable();

        var result = products.ApplyFilter(10).ToList();

        Assert.Equal(new[] { 1, 2 }, result.Select(p => p.Id));
        Assert.All(result, p => Assert.Equal(10, p.CategoryId));
    }

    [Fact]
    public void ApplyFilter_WithNullCategory_ReturnsAllProducts()
    {
        var products = new[]
        {
            CreateProduct(id: 1, name: "Phone", description: "Pocket device", price: 20, categoryId: 10),
            CreateProduct(id: 2, name: "Novel", description: "Paper fiction", price: 5, categoryId: 20)
        }.AsQueryable();

        var result = products.ApplyFilter(null).ToList();

        Assert.Equal(new[] { 1, 2 }, result.Select(p => p.Id));
    }

    [Fact]
    public void ApplySearch_IsCaseInsensitiveOnName()
    {
        var products = new[]
        {
            CreateProduct(id: 1, name: "Gaming Laptop", description: "Powerful machine", price: 1200),
            CreateProduct(id: 2, name: "Office Chair", description: "Comfortable seat", price: 200)
        }.AsQueryable();

        var result = products.ApplySearch("LAPTOP").ToList();

        var product = Assert.Single(result);
        Assert.Equal(1, product.Id);
    }

    [Fact]
    public void ApplySearch_IsCaseInsensitiveOnDescription()
    {
        var products = new[]
        {
            CreateProduct(id: 1, name: "Gaming Laptop", description: "Portable workstation", price: 1200),
            CreateProduct(id: 2, name: "Office Chair", description: "Comfortable seat", price: 200)
        }.AsQueryable();

        var result = products.ApplySearch("portable").ToList();

        var product = Assert.Single(result);
        Assert.Equal(1, product.Id);
    }

    [Fact]
    public void ApplySearch_MatchesNameOrDescription()
    {
        var products = new[]
        {
            CreateProduct(id: 1, name: "Gaming Laptop", description: "Powerful machine", price: 1200),
            CreateProduct(id: 2, name: "Cookbook", description: "Recipes and tips", price: 30),
            CreateProduct(id: 3, name: "Camera", description: "Lens kit", price: 700)
        }.AsQueryable();

        var result = products.ApplySearch("recipes").ToList();

        var product = Assert.Single(result);
        Assert.Equal(2, product.Id);
    }

    [Fact]
    public void ApplySearch_MultipleWords_RequiresEveryWordToMatch()
    {
        var products = new[]
        {
            CreateProduct(id: 1, name: "Gaming Laptop", description: "Fast portable computer", price: 1200),
            CreateProduct(id: 2, name: "Office Laptop", description: "Reliable workstation", price: 900),
            CreateProduct(id: 3, name: "Camera", description: "Portable lens kit", price: 700)
        }.AsQueryable();

        var result = products.ApplySearch("laptop portable").ToList();

        var product = Assert.Single(result);
        Assert.Equal(1, product.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ApplySearch_EmptyValue_ReturnsAllProducts(string? search)
    {
        var products = new[]
        {
            CreateProduct(id: 1, name: "Gaming Laptop", description: "Fast portable computer", price: 1200),
            CreateProduct(id: 2, name: "Cookbook", description: "Recipes and tips", price: 30)
        }.AsQueryable();

        var result = products.ApplySearch(search).ToList();

        Assert.Equal(new[] { 1, 2 }, result.Select(p => p.Id));
    }

    [Theory]
    [InlineData(1, 3, new[] { 1, 2, 3 })]
    [InlineData(2, 3, new[] { 4, 5, 6 })]
    [InlineData(4, 3, new[] { 10 })]
    [InlineData(5, 3, new int[0])]
    public void ApplyPaging_SlicesUsingOneBasedPageIndex(int pageIndex, int pageSize, int[] expectedIds)
    {
        var products = Enumerable.Range(1, 10)
            .Select(id => CreateProduct(id, $"Product {id}", $"Description {id}", id))
            .AsQueryable();

        var result = products.ApplyPaging(pageIndex, pageSize).ToList();

        Assert.Equal(expectedIds, result.Select(p => p.Id));
    }

    private static Product CreateProduct(
        int id,
        string name,
        string description,
        decimal price,
        int categoryId = 1)
    {
        var product = new Product(name, description, categoryId, price + 1, price);
        product.Id = id;
        return product;
    }
}
