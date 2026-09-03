namespace Ecom.infrastructure.Services;

public record UploadedFile(string FileName, Stream Content);

public interface IImageManagementServices
{
    Task<List<string>> AddImageAsync(IEnumerable<UploadedFile> files, string src);
    void DeleteImageAsync(string src);
}
