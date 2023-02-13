using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using BccCode.DocumentationSite.Services;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Caching.Memory;
using Azure;
using IdentityServer4.Extensions;
using System.Text.RegularExpressions;

namespace BccCode.DocumentationSite.Controllers
{
    [Route("getSAS")]
    [ApiController]
    public class SASUriController : ControllerBase
    {

        private readonly IGetMembersInterface _getmembers;

        public SASUriController(IGetMembersInterface getmembers)
        {
            this._getmembers = getmembers;
        }

        [HttpGet]
        public async Task<string> SASURI(string? repo = "")
        {
            if (Request.Headers["X-MS-CLIENT-PRINCIPAL-ID"].ToString().IsNullOrEmpty())
            {
                Response.Redirect(new PathString("/.auth/login/github") + "?post_login_redirect_uri=/getSAS");
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

            //Using getmembers cache to see if user have access to repo or repo doesnt exsist
            try
            {
                var access = await _getmembers.GetUsersInRepo("", repo);
                if (access.IsNullOrEmpty())
                    return "Repository doesnt exsists";
                if (access.Contains(404))
                    goto Token;
                if (access.Contains(int.Parse(Request.Headers["X-MS-CLIENT-PRINCIPAL-ID"].ToString())))
                    goto Token;
                else
                    return "No access";
            }
            catch (Exception e)
            {
                return e.Message;
            }

        Token:
            ////Initiallize the SASToken class
            //SASToken token = new SASToken();
            //try
            //{
            //    //Gets the user delegation SAS uri for the repository(container)
            //    string SASuri = await token.GetUserDelegationSasContainer(repo);

            //    return SASuri;
            //}
            //catch (RequestFailedException e)
            //{
            //    return e.Message;
            //}
            return "sas?sas";
        }
    }
}
