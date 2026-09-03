using Ecom.infrastructure.Repositries.Service;
using Ecom.infrastructure.Services;
using Microsoft.Extensions.FileProviders;
using Moq;

namespace Ecom.UnitTests;

public class ImageManagementServiceTests
{
    [Fact]
    public void DeleteImageAsync_WhenFileDoesNotExist_DoesNotThrow()
    {
        var fileInfoMock = new Mock<IFileInfo>();
        fileInfoMock.Setup(f => f.Exists).Returns(false);

        var fileProviderMock = new Mock<IFileProvider>();
        fileProviderMock.Setup(f => f.GetFileInfo(It.IsAny<string>())).Returns(fileInfoMock.Object);

        var service = new ImageManagementService(fileProviderMock.Object);

        var exception = Record.Exception(() => service.DeleteImageAsync("missing.jpg"));
        Assert.Null(exception);
    }
}
