using Ecom.Core.DTO;

namespace Ecom.UnitTests;

public class ProductParamsTests
{
    [Fact]
    public void Defaults_PageIndexAndPageSizeAreOne()
    {
        var productParams = new ProductParams();

        Assert.Equal(1, productParams.PageIndex);
        Assert.Equal(1, productParams.PageSize);
        Assert.Null(productParams.Sort);
        Assert.Null(productParams.CategoryId);
        Assert.Null(productParams.Search);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void PageIndex_BelowOne_ClampsToOne(int value)
    {
        var productParams = new ProductParams { PageIndex = value };

        Assert.Equal(1, productParams.PageIndex);
    }

    [Fact]
    public void PageIndex_PositiveValue_IsPreserved()
    {
        var productParams = new ProductParams { PageIndex = 4 };

        Assert.Equal(4, productParams.PageIndex);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void PageSize_BelowOne_ClampsToOne(int value)
    {
        var productParams = new ProductParams { PageSize = value };

        Assert.Equal(1, productParams.PageSize);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(3, 3)]
    [InlineData(6, 6)]
    [InlineData(7, 6)]
    [InlineData(100, 6)]
    public void PageSize_IsCappedAtSix(int value, int expected)
    {
        var productParams = new ProductParams { PageSize = value };

        Assert.Equal(expected, productParams.PageSize);
    }
}
