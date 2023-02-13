using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.IdentityModel.Tokens;
using BccCode.DocumentationSite.Services;
using Microsoft.Extensions.Caching.Memory;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace BccCode.DocumentationSite.Controllers
{
    [Route("gitrepo")]
    [ApiController]
    public class GitRepoController : ControllerBase
    {

        private readonly IGetMembersInterface _getmembers;

        public GitRepoController(IGetMembersInterface getmembers)
        {
            this._getmembers = getmembers;
        }

        [HttpGet]
        public async Task<object> auth(string? repo = "")
        {
            if (HttpContext.Request.Headers["X-MS-CLIENT-PRINCIPAL-ID"].ToString().IsNullOrEmpty())
            {
                Response.Redirect(new PathString("/.auth/login/github") + "?post_login_redirect_uri=/gitrepo");
                return "User isnt logged in";
            }
            if (repo == null || repo == "")
            {
                return "No repository was specified";
            }
            if (!Regex.IsMatch(repo, @"^[a-zA-Z0-9_.-]+$"))
            {
                return "Repository name is invalid!";
            }

            //Checks if repo exsists in the cache to skip the azure vault function needed to get a token for that repo
            if (_getmembers.GetUsersInRepo("", repo).Result.IsNullOrEmpty())
            {

                //Calling this method to get github token using the azure vault pem file
                string token = await _getmembers.GetTokenFromAzurePem();

                //Calling method to retrive users who have access to the repo
                var users = await _getmembers.GetUsersInRepo(token, repo);

                //Checks if repo is not public (if list contains the element "404", repo is public)
                if (!(users.Contains(404)))
                    //If the list is an empty list the repository doesnt exsists
                    if (users.IsNullOrEmpty())
                        return "Repository doesnt exsists";
                    else
                        //Returns if the current logged user exsists whitin the list of allowed people
                        return users.Contains(int.Parse(Request.Headers["X-MS-CLIENT-PRINCIPAL-ID"]));
                else
                {
                    return true;
                }
            }
            else
            {
                //Checks if cached repo is public or not
                if ((await _getmembers.GetUsersInRepo("", repo)).Contains(404))
                    return true;
                else
                //Checks if user exsists in the cached repo
                return (await _getmembers.GetUsersInRepo("",repo)).Contains(int.Parse(Request.Headers["X-MS-CLIENT-PRINCIPAL-ID"]));
            }
        }
    }
}
