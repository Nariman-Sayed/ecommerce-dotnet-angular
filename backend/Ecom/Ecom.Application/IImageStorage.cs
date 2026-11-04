namespace Ecom.Application;

public interface IImageStorage
{
    Task<IReadOnlyList<string>> SaveAsync(IEnumerable<ImageUpload> files, string folderName);

    void Delete(string imagePath);
}

public sealed record ImageUpload(string FileName, Stream Content);
