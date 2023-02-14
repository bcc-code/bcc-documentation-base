using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.Storage.Blobs;
using BccCode.DocumentationSite.Services;
using IdentityServer4.Extensions;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using Yarp.ReverseProxy.Forwarder;

namespace BccCode.DocumentationSite.Models
{
    public class CustomTransformer : HttpTransformer
    {
        private readonly IConfiguration config;
        private readonly IMemoryCache cache;
        private readonly IGetMembersInterface getmembers;
        public CustomTransformer(IConfiguration config, IMemoryCache cache, IGetMembersInterface getmembers)
        {
            this.config = config;
            this.cache = cache;
            this.getmembers = getmembers;
        }

        public override async ValueTask TransformRequestAsync(HttpContext httpContext,HttpRequestMessage proxyRequest, string destinationPrefix)
        {
            var credential = new DefaultAzureCredential();
            var storageUrl = new EnviromentVar(config).GetEnviromentVariable("StorageUrl");
            var homePage = new EnviromentVar(config).GetEnviromentVariable("HomePageContainer");
            SASToken token = new SASToken(credential, storageUrl, cache);
            var path = httpContext.Request.Path.Value!;
            try
            {

                #region container naming check
                var containerName = path.Substring(path.IndexOf('/') + 1, path.IndexOf('/', 1) - 1);
                if (containerName == null || containerName == "")
                {
                    httpContext.Response.StatusCode = 404;
                    return; ;
                }
                if (!Regex.IsMatch(containerName, @"^[a-zA-Z0-9_.-]+$"))
                {
                    httpContext.Response.StatusCode = 404;
                    return; ;
                }
                #endregion

                #region home page redirect
                // replacing '.' with '-' to avoid naming errors in azure
                if (containerName.Contains('.'))
                {
                    containerName = containerName.Replace('.', '-');
                }

                //Checks if container name is home page 
                if (containerName == homePage)
                {
                    string HPSASToken = await token.GetUserDelegationSasContainer(containerName);
                    path = $"/{containerName}{path.Substring(containerName.Length + 1)}";
                    if (path.Contains('#'))
                    {
                        path.Remove(path.IndexOf('#'));
                    }
                    if (path.EndsWith('/'))
                    {
                        path = path + "index.html";
                    }
                    proxyRequest.RequestUri = RequestUtilities.MakeDestinationAddress(storageUrl, path, new QueryString(HPSASToken));
                    return; ;
                }

                //Check if the container exsists else send you to home page
                Uri container = new Uri(storageUrl + containerName);
                BlobContainerClient blobcontainer = new BlobContainerClient(container, credential);
                if (!(await blobcontainer.ExistsAsync()))
                {
                    string HPSASToken = await token.GetUserDelegationSasContainer(homePage);
                    path = $"/{homePage}/";
                    if (path.Contains('#'))
                    {
                        path.Remove(path.IndexOf('#'));
                    }
                    if (path.EndsWith('/'))
                    {
                        path = path + "index.html";
                    }
                    proxyRequest.RequestUri = RequestUtilities.MakeDestinationAddress(storageUrl, path, new QueryString(HPSASToken));
                    return; ;
                }
                #endregion

                #region user signin confirmation
                if (httpContext.Request.Headers["X-MS-CLIENT-PRINCIPAL-ID"].ToString().IsNullOrEmpty())
                {
                    httpContext.Response.Redirect(new PathString("/.auth/login/github") + $"?post_login_redirect_uri={path}");
                    return; ;
                }
                #endregion

                #region authenticate user access
                //Checks if repo exsists in the cache to skip the azure vault function needed to get a token for that repo
                if (getmembers.GetUsersInRepo("", containerName).Result.IsNullOrEmpty())
                {

                    //Calling this method to get github token using the azure vault pem file
                    string gitToken = await getmembers.GetTokenFromAzurePem();

                    //Calling method to retrive users who have access to the repo
                    var users = await getmembers.GetUsersInRepo(gitToken, containerName);

                    //Checks if repo is not public (if list contains the element "404", repo is public)
                    if (!(users.Contains(404)))
                    {
                        //If the list is an empty list the repository doesnt exsists
                        if (users.IsNullOrEmpty())
                        {
                            httpContext.Response.StatusCode = 403;
                            return; ;
                        }
                        //Returns if the current logged user exsists whitin the list of allowed people
                        else
                        {
                            if (!users.Contains(int.Parse(httpContext.Request.Headers["X-MS-CLIENT-PRINCIPAL-ID"])))
                            {
                                httpContext.Response.StatusCode = 403;
                                return; ;
                            }
                        }
                    }
                }
                else
                {
                    //Checks if cached repo is public or not
                    if (!(await getmembers.GetUsersInRepo("", containerName)).Contains(404))
                    {
                        //Checks if user exsists in the cached repo
                        if (!(await getmembers.GetUsersInRepo("", containerName)).Contains(int.Parse(httpContext.Request.Headers["X-MS-CLIENT-PRINCIPAL-ID"])))
                        {
                            httpContext.Response.StatusCode = 403;
                            return; ;
                        }
                    }
                }
                #endregion

                //Gets SAS token for container and adds it in the proxy
                string SASToken = await token.GetUserDelegationSasContainer(containerName);
                if (path.Contains('#'))
                {
                    path.Remove(path.IndexOf('#'));
                }
                if (path.EndsWith('/'))
                {
                    path = path + "index.html";
                }
                proxyRequest.RequestUri = RequestUtilities.MakeDestinationAddress(storageUrl, path, new QueryString(SASToken));
            }
            catch (Exception e)
            {
                //If referencing base root redirect to home page
                if (e.Message.Contains("Length"))
                {
                    string SASToken = await token.GetUserDelegationSasContainer(homePage);
                    path = $"/{homePage}/";
                    if (path.Contains('#'))
                    {
                        path.Remove(path.IndexOf('#'));
                    }
                    if (path.EndsWith('/'))
                    {
                        path = path + "index.html";
                    }
                    proxyRequest.RequestUri = RequestUtilities.MakeDestinationAddress(storageUrl, path, new QueryString(SASToken));
                }
                else
                httpContext.Response.StatusCode = 400;
            }
           
        }
    }
}
