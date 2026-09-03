using Microsoft.AspNetCore.Http;

namespace Ecom.Core.Services
{
    public interface IImageManagementServices
    {
        Task<List<string>> AddImageAsync(
            IFormFileCollection files,
            string src);

        void DeleteImageAsync(string src);
    }
}