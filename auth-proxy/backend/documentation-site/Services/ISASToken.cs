namespace BccCode.DocumentationSite.Services
{
    public interface ISASToken
    {
        Task<string> GetUserDelegationSasContainer(string containerName);
    }
}
