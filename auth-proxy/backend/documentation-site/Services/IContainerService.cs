namespace BccCode.DocumentationSite.Services
{
    public interface IContainerService
    {
        Task<string> GetUserDelegationSasContainer(string containerName);
        Task<List<string>> GetContainersList();
        Task<List<string>> GetBlobsList(string container);
        Task UpdateBlobsList(string container);
        Task<bool> IsPublic(string ContainerName);
    }
}
