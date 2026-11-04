using Ecom.infrastructure.Services;
using Microsoft.Extensions.FileProviders;
using Moq;

namespace Ecom.UnitTests;

public class ImageStorageTests
{
    [Fact]
    public void Delete_WhenFileDoesNotExist_DoesNotThrow()
    {
        var fileInfoMock = new Mock<IFileInfo>();
        fileInfoMock.Setup(f => f.Exists).Returns(false);

        var fileProviderMock = new Mock<IFileProvider>();
        fileProviderMock.Setup(f => f.GetFileInfo(It.IsAny<string>())).Returns(fileInfoMock.Object);

        var storage = new ImageStorage(fileProviderMock.Object);

        var exception = Record.Exception(() => storage.Delete("missing.jpg"));
        Assert.Null(exception);
    }
}
