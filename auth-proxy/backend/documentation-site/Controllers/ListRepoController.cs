using Azure;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BccCode.DocumentationSite.Services;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text;

namespace BccCode.DocumentationSite.Controllers
{
    [Route("repolist")]
    [ApiController]
    public class ListRepoController : ControllerBase
    {

        private readonly IGetMembersInterface _getmembers;

        public ListRepoController(IGetMembersInterface getmembers)
        {
            this._getmembers = getmembers;
        }

        [HttpGet]
        public async Task<object> GetRepoList()
        {

            if (Request.Headers["X-MS-CLIENT-PRINCIPAL-ID"].ToString().IsNullOrEmpty())
            {
                Response.Redirect(new PathString("/.auth/login/github") + "?post_login_redirect_uri=/repolist");
                return "User isnt logged in";
            }

            #region List containers(repositories) in blob
            //List for containers that are in blob
            List<string> containers = new List<string>();

            //Gets the blob storage
            DefaultAzureCredential credential = new DefaultAzureCredential();
            var blobEndpoint = new Uri($"https://docsitestorageaccount.blob.core.windows.net");
            var blobClient = new BlobServiceClient(blobEndpoint, credential);

            //Lists containers in the blob and adds them to the containers list
            try
            {
                var result = blobClient.GetBlobContainersAsync().AsPages();
                await foreach (Azure.Page<BlobContainerItem> containerPage in result)
                {
                    foreach (BlobContainerItem containerItem in containerPage.Values)
                    {
                        containers.Add(containerItem.Name);
                    }
                }
            }
            catch (RequestFailedException e)
            {
                return e.Message;
            }
            #endregion

            #region Get repositories user have access to from the container list
            //Get an auth cookie to use for http client calls
            HttpContext.Request.Cookies.TryGetValue("AppServiceAuthSession", out string? cookie);
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36");
            client.DefaultRequestHeaders.Add("Cookie", $"AppServiceAuthSession={cookie}");

            //List for repositories user have access to
            List<string> accessableRepos = new List<string>();
            foreach (var container in containers)
            {
                //Get if user have access to repo from this api
                //**see if can call method locally insted api**
                var isMemberOfRepo = await client.GetAsync($"https://{Request.Host.ToString()}/gitrepo?repo={container}");
                var answer = await isMemberOfRepo.Content.ReadAsStringAsync();
                //Checks if answer from the api is bool and is "true"
                if (bool.TryParse(answer, out bool result))
                {
                    if (result)
                    {
                        accessableRepos.Add(container);
                    }
                }
            }
            #endregion

            #region Get SAS token to each repository (container in the blob) the user have access to
            //For each repo in the accessableRepos create a corisponding SAS token and generates a dictionary that contains the repo(container) name as "key" and the SAS token as value of said key.
            Dictionary<string, string> RepoSASList = new Dictionary<string, string>();
            foreach (string repo in accessableRepos)
            {
                var SAStokenapi = await client.GetAsync($"https://{Request.Host.ToString()}/getSAS?repo={repo}");
                var SAStoken = await SAStokenapi.Content.ReadAsStringAsync();
                SAStoken = SAStoken.Substring(SAStoken.IndexOf('?') + 1);
                RepoSASList.Add(repo, SAStoken);
            }
            #endregion

            //Returns the repo:SAStoken pairs
            return RepoSASList;

        }
    }
}
