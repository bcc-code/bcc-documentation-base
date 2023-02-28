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
using Yarp.ReverseProxy.Model;
using Yarp.ReverseProxy.Transforms;

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
            var homePage = "home";
            SASToken token = new SASToken(credential, storageUrl, cache);
            var path = httpContext.Request.Path.Value!;
            try
            {
                if (destinationPrefix == (storageUrl + homePage + "/"))
                {
                    //Appending the index.html to the sub path in case the subpath isnt refering to a file whitin the container
                    if (!path.Contains(".") && !path.EndsWith("/"))
                    {
                        path = path + "/index.html";
                    }
                    else if (!path.Contains(".") && path.EndsWith("/"))
                    {
                        path = path + "index.html";
                    }
                    string HPSASToken = await token.GetUserDelegationSasContainer(homePage);
                    var test = storageUrl + homePage + "/";
                    proxyRequest.RequestUri = RequestUtilities.MakeDestinationAddress(test, path, new QueryString(HPSASToken));
                    return;
                }
            }
            catch (Exception e)
            {
                return;
            }


            try
            {
                #region container naming check
                //Extract container name from the path which appears after the first '/' in the path
                var containerName = path.Split('/')[1];
                if (containerName.IsNullOrEmpty())
                {
                    containerName = homePage;
                }
                //Does input validation for the container name
                if (!Regex.IsMatch(containerName, @"^[a-zA-Z0-9_.-]+$"))
                {
                    httpContext.Response.StatusCode = 404;
                    return;
                }

                // replacing '.' with '-' to avoid naming errors in azure storage
                if (containerName.Contains('.'))
                {
                    containerName = containerName.Replace('.', '-');
                }
                #endregion

                #region SubPath check
                //The path after the container name
                var subPath = string.Join('/', path.Split("/").Skip(2));

                //Appending the index.html to the sub path in case the subpath isnt refering to a file whitin the container
                if (!subPath.Contains(".") && !subPath.EndsWith("/"))
                {
                    subPath = subPath + "/index.html";
                }
                else if (!subPath.Contains(".") && subPath.EndsWith("/"))
                {
                    subPath = subPath + "index.html";
                }
                #endregion

                #region home page redirect
                //Checks if container name is home page 
                if (containerName == homePage)
                {
                    string HPSASToken = await token.GetUserDelegationSasContainer(containerName);
                    if (subPath.StartsWith("/"))
                        path = $"/{homePage}{subPath}";
                    else
                        path = $"/{homePage}/{subPath}";
                    var test = storageUrl + homePage + "/";
                    if (!subPath.StartsWith("/"))
                        subPath = "/" + subPath;
                    proxyRequest.RequestUri = RequestUtilities.MakeDestinationAddress(test , subPath, new QueryString(HPSASToken));
                    return;
                }
                #endregion

                #region discord page redirect
                //Checks if container name is discord
                if (containerName == "discord")
                {
                    string DSASToken = await token.GetUserDelegationSasContainer(containerName);
                    if (subPath.StartsWith("/"))
                        path = $"/discord{subPath}";
                    else
                        path = $"/discord/{subPath}";
                    proxyRequest.RequestUri = RequestUtilities.MakeDestinationAddress(storageUrl, path, new QueryString(DSASToken));
                    return; ;
                }
                #endregion

                #region Container Check
                //Check if the container exsists else send you to home page 404 page
                Uri container = new Uri(storageUrl + containerName);
                BlobContainerClient blobcontainer = new BlobContainerClient(container, credential);
                if (!(await blobcontainer.ExistsAsync()))
                {
                    string HPSASToken = await token.GetUserDelegationSasContainer(homePage);
                    path = $"/{homePage}/404.html";

                    proxyRequest.RequestUri = RequestUtilities.MakeDestinationAddress(storageUrl, path, new QueryString(HPSASToken));
                    return;
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
                //Checks if repository members exsists in the cache
                if ((await getmembers.GetUsersInRepo("", containerName)).IsNullOrEmpty())
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
                //if (path.Contains('#'))
                //{
                //    path.Remove(path.IndexOf('#'));
                //}
                if (path.EndsWith('/'))
                {
                    path = path + "index.html";
                }
                proxyRequest.RequestUri = RequestUtilities.MakeDestinationAddress(storageUrl, path, new QueryString(SASToken));
            }
            catch (Exception e)
            {
                //If referencing base root redirect to home page
                if (e.GetType() == (typeof (ArgumentNullException)))
                {
                    string SASToken = "";
                    if (!path.EndsWith("/"))
                    {
                        path = path + "/";
                        var containerName = path.Split('/')[1];
                        SASToken = await token.GetUserDelegationSasContainer(containerName);
                    }
                    if (SASToken == "" || !SASToken.Contains("?"))
                    {
                        path = $"/{homePage}/";
                        SASToken = await token.GetUserDelegationSasContainer(homePage);
                    }
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
