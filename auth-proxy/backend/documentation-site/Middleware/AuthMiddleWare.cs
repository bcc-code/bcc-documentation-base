using Azure;
using Azure.Identity;
using BccCode.DocumentationSite.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace BccCode.DocumentationSite.Middleware
{
    public class AuthMiddleWare
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _config;

        public AuthMiddleWare(RequestDelegate next, IMemoryCache cache, IConfiguration config)
        {
            _next = next;
            _cache = cache;
            _config = config;
        }

        public async Task InvokeAsync(HttpContext context, IAuthenticationService authenticationService)
        {
            try
            {

                var path = context.Request.Path;
                //Extract container name from the path which appears after the first '/' in the path
                var containerName = path.Value!.Split('/')[1];
                var credential = new DefaultAzureCredential();
                var envVar = new EnviromentVar(_config);
                ContainerService cService = new ContainerService(credential, envVar.GetEnviromentVariable("StorageUrl"), _cache);

                var authmethod = await cService.AuthProvider(containerName);
                if (authmethod == "azuread")
                {
                    var result = await authenticationService.AuthenticateAsync(context, "AzureAd");
                    if (!result.Succeeded)
                    {
                        await authenticationService.ChallengeAsync(context, "AzureAd", new AuthenticationProperties { RedirectUri = $"{path}" });
                        return;
                    }
                }
                if (authmethod == "portal")
                {
                    var result = await authenticationService.AuthenticateAsync(context, "Portal");
                    if (!result.Succeeded)
                    {
                        await authenticationService.ChallengeAsync(context, "Portal", new AuthenticationProperties { RedirectUri = $"{path}" });
                        return;
                    }
                }

                await _next(context);
            }
            catch (Exception e)
            {
                context.Response.Redirect(new PathString("/"));
                return;
            }
        }
    }
}