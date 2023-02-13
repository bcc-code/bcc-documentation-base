using System.Threading.Tasks;
using System.Collections.Generic;

namespace BccCode.DocumentationSite.Services
{
    public interface IGetMembersInterface
    {
        Task<string> GetTokenFromAzurePem();
        Task<List<int>> GetUsersInRepo(string token = "", string repo = "");
    }
}
