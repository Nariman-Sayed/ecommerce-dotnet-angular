using Ecom.Core.DTO;

namespace Ecom.UnitTests;

public class PaginationTests
{
    [Fact]
    public void Constructor_ExposesMetadataAndData()
    {
        var data = new[] { 1, 2, 3 };

        var pagination = new Pagination<int>(pageIndex: 2, pageSize: 3, totalCount: 10, data);

        Assert.Equal(2, pagination.PageIndex);
        Assert.Equal(3, pagination.PageSize);
        Assert.Equal(10, pagination.TotalCount);
        Assert.Equal(data, pagination.Data);
    }

    [Fact]
    public void Constructor_CopiesDataSoLaterSourceChangesDoNotAffectThePage()
    {
        var data = new List<int> { 1, 2, 3 };

        var pagination = new Pagination<int>(1, 3, 3, data);
        data.Add(4);

        Assert.Equal(new[] { 1, 2, 3 }, pagination.Data);
    }

    [Fact]
    public void Data_IsReadOnly()
    {
        var pagination = new Pagination<int>(1, 3, 3, new[] { 1, 2, 3 });

        var list = Assert.IsAssignableFrom<IList<int>>(pagination.Data);

        Assert.True(list.IsReadOnly);
        Assert.Throws<NotSupportedException>(() => list.Add(4));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_PageIndexBelowOne_Throws(int pageIndex)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new Pagination<int>(pageIndex, 1, 0, Array.Empty<int>()));

        Assert.Equal("pageIndex", exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_PageSizeBelowOne_Throws(int pageSize)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new Pagination<int>(1, pageSize, 0, Array.Empty<int>()));

        Assert.Equal("pageSize", exception.ParamName);
    }

    [Fact]
    public void Constructor_NegativeTotalCount_Throws()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new Pagination<int>(1, 1, -1, Array.Empty<int>()));

        Assert.Equal("totalCount", exception.ParamName);
    }

    [Fact]
    public void Constructor_NullData_Throws()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new Pagination<int>(1, 1, 0, null!));

        Assert.Equal("data", exception.ParamName);
    }
}
