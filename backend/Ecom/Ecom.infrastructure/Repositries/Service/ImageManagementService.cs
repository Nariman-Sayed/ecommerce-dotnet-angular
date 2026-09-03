using Ecom.infrastructure.Services;
using Microsoft.Extensions.FileProviders;

namespace Ecom.infrastructure.Repositries.Service;

public class ImageManagementService : IImageManagementServices
{
    private readonly IFileProvider fileProvider;

    public ImageManagementService(IFileProvider fileProvider)
    {
        this.fileProvider = fileProvider;
    }

    public async Task<List<string>> AddImageAsync(IEnumerable<UploadedFile> files, string src)
    {
        var savedImagePaths = new List<string>();
        var imageDirectory = Path.Combine("wwwroot", "Images", src);

        if (!Directory.Exists(imageDirectory))
            Directory.CreateDirectory(imageDirectory);

        foreach (var file in files)
        {
            var imagePath = $"/Images/{src}/{file.FileName}";
            var fullPath = Path.Combine(imageDirectory, file.FileName);

            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.Content.CopyToAsync(stream);

            savedImagePaths.Add(imagePath);
        }

        return savedImagePaths;
    }

    public void DeleteImageAsync(string src)
    {
        var info = fileProvider.GetFileInfo(src);
        if (info.Exists && info.PhysicalPath is not null)
            File.Delete(info.PhysicalPath);
    }
}
