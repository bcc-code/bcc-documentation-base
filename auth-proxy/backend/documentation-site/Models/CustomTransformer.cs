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
        private readonly IMemoryCache cache;
        private readonly IGetMembersInterface getmembers;
        private readonly ILogger logger;

        public CustomTransformer(IMemoryCache cache, IGetMembersInterface getmembers, ILoggerFactory logger)
        {
            this.cache = cache;
            this.getmembers = getmembers;
            this.logger = logger.CreateLogger<CustomTransformer>();
        }

        public override async ValueTask TransformRequestAsync(HttpContext httpContext,HttpRequestMessage proxyRequest, string destinationPrefix)
        {
            var credential = new DefaultAzureCredential();
            ContainerService token = new ContainerService(credential, destinationPrefix, cache);

            var path = httpContext.Request.Path.Value!;
            //Extract container name from the path which appears after the first '/' in the path
            var containerName = path.Split('/')[1];
            //The path after the container name
            var subPath = "/" + string.Join('/', path.Split("/").Skip(2));
            try
            {
                #region container naming check
                if (containerName.IsNullOrEmpty())
                {
                    containerName = "home";
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

                //Checking if endpoint is discord or the home page to see if user need authentication
                if (containerName != "home" && containerName != "discord")
                {
                    #region Container Check
                    //Gets List of the containers that exsist in the storage account
                    var containerList = await token.GetContainersList();
                    //Check if the container exsists else send you to home page
                    if (!containerList.Contains(containerName))
                    {
                        string HPSASToken = await token.GetUserDelegationSasContainer("home");
                        if ((await token.GetBlobsList("home")).Contains(path))
                        {
                            path = $"/home{path}";
                            proxyRequest.RequestUri = RequestUtilities.MakeDestinationAddress(destinationPrefix, path, new QueryString(HPSASToken));
                        }
                        else
                            proxyRequest.RequestUri = RequestUtilities.MakeDestinationAddress(destinationPrefix, "/home/index.html", new QueryString(HPSASToken));
                        return;
                    }
                    #endregion

                    #region user signin confirmation
                    if (httpContext.Request.Headers["X-MS-CLIENT-PRINCIPAL-ID"].ToString().IsNullOrEmpty())
                    {
                        httpContext.Response.Redirect(new PathString("/.auth/login/github") + $"?post_login_redirect_uri={path}");
                        return;
                    }
                    #endregion

                    logger.LogWarning("----------------------------------PUBLIC FILE EXSISTENCE:-----------------------------------");

                    var answer = await token.IsPublic(containerName);
                    if (answer)
                        logger.LogWarning("true");
                    else
                        logger.LogWarning("false");

                    if (containerName != "bcc-core-api")
                    {
                        #region authenticate user access
                        //Gets the cached list of users who have access to the repository
                        var usersInRepo = await getmembers.GetUsersInRepo("", containerName);

                        //Checks if repository members exsists in the cache
                        if (usersInRepo.IsNullOrEmpty())
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
                                    return;
                                }
                                //Returns if the current logged user exsists whitin the list of allowed people
                                else
                                {
                                    if (!users.Contains(int.Parse(httpContext.Request.Headers["X-MS-CLIENT-PRINCIPAL-ID"])))
                                    {
                                        httpContext.Response.StatusCode = 403;
                                        return;
                                    }
                                }
                            }
                        }
                        else
                        {
                            //Checks if cached repo is public or not
                            if (!usersInRepo.Contains(404))
                            {
                                //Checks if user exsists in the cached repo
                                if (!usersInRepo.Contains(int.Parse(httpContext.Request.Headers["X-MS-CLIENT-PRINCIPAL-ID"])))
                                {
                                    httpContext.Response.StatusCode = 403;
                                    return;
                                }
                            }
                        }
                        #endregion
                    }
                }

                #region SubPath check
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

                #region redirect to documentation page
                //Gets SAS token for container and adds it in the proxy
                string SASToken = await token.GetUserDelegationSasContainer(containerName);
                if ((await token.GetBlobsList(containerName)).Contains(subPath))
                {
                    path = $"/{containerName}{subPath}";
                    proxyRequest.RequestUri = RequestUtilities.MakeDestinationAddress(destinationPrefix, path, new QueryString(SASToken));
                }
                else
                    proxyRequest.RequestUri = RequestUtilities.MakeDestinationAddress(destinationPrefix, $"/{containerName}/index.html", new QueryString(SASToken));
                #endregion

            }
            catch (Exception e)
            {
                string HPSASToken = await token.GetUserDelegationSasContainer("home");
                proxyRequest.RequestUri = RequestUtilities.MakeDestinationAddress(destinationPrefix, "/home/index.html", new QueryString(HPSASToken));
            }
        }
    }
}
