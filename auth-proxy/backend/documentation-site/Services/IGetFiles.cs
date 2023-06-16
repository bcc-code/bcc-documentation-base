using Microsoft.AspNetCore.Mvc;

namespace BccCode.DocumentationSite.Services
{
    public interface IGetFiles
    {
        Task<string> UploadPagesToStorage(string repo, IFormFile zip, bool isPublic = false, string auth = "github");
    }
}
