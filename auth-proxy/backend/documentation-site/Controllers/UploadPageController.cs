using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using BccCode.DocumentationSite.Services;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BccCode.DocumentationSite.Controllers
{
    [Route("UploadDoc")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "github", Policy = "githubpolicy")]
    public class UploadPageController : ControllerBase
    {
        private readonly IGetFiles _files;

        public UploadPageController(IGetFiles Files)
        {
            this._files = Files;
        }

        [HttpPost]
        public async Task<string> PushDocToContainer(IFormFile Docs, string discord = "")
        {
            try
            {
                //Gets Claims from bearer token
                var id = HttpContext.User.Identity as ClaimsIdentity;
                //Looks if repository in the claim matches the repository to which you upload your artifact to
                var repoclaim = id!.FindFirst("repository")!.Value;
                var repo = repoclaim.Substring(repoclaim.IndexOf('/') + 1);
                if (repo == null || repo == "")
                {
                    return "No repository was specified";
                }
                //checks if repo name rgex pattern is valid
                if (!Regex.IsMatch(repo, @"^[a-zA-Z0-9_.-]+$"))
                {
                    return "Repository name is invalid!";
                }
                #region Discord endpoint
                //Checks discord claim
                if (discord == "yes" && repo == "community-tools")
                {
                    return await _files.UploadPagesToStorage((repo + discord), Docs);
                }
                #endregion
                else
                {
                    return await _files.UploadPagesToStorage(repo, Docs);
                }
            }
            catch
            {
                return "Invalid token";
            }
        }
    }
}
