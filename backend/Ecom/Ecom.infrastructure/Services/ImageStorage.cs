using Ecom.Application;
using Microsoft.Extensions.FileProviders;

namespace Ecom.infrastructure.Services;

public class ImageStorage : IImageStorage
{
    private readonly IFileProvider _fileProvider;

    public ImageStorage(IFileProvider fileProvider)
    {
        _fileProvider = fileProvider;
    }

    public async Task<IReadOnlyList<string>> SaveAsync(IEnumerable<ImageUpload> files, string folderName)
    {
        var savedImagePaths = new List<string>();

        foreach (var file in files)
        {
            var imagePath = $"/Images/{folderName}/{file.FileName}";
            var physicalPath = _fileProvider.GetFileInfo(imagePath).PhysicalPath!;

            Directory.CreateDirectory(Path.GetDirectoryName(physicalPath)!);

            using var stream = new FileStream(physicalPath, FileMode.Create);
            await file.Content.CopyToAsync(stream);

            savedImagePaths.Add(imagePath);
        }

        return savedImagePaths;
    }

    public void Delete(string imagePath)
    {
        var info = _fileProvider.GetFileInfo(imagePath);
        if (info.Exists && info.PhysicalPath is not null)
            File.Delete(info.PhysicalPath);
    }
}
