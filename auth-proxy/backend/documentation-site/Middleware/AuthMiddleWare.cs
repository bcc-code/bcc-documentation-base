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
                #region a
                var path = context.Request.Path;
                //Extract container name from the path which appears after the first '/' in the path
                var containerName = path.Value!.Split('/')[1];
                var credential = new DefaultAzureCredential();
                var envVar = new EnviromentVar(_config);
                ContainerService cService = new ContainerService(credential, envVar.GetEnviromentVariable("StorageUrl"), _cache);

                if ((await cService.AuthProvider(containerName)) == "azuread")
                {
                    var result = await authenticationService.AuthenticateAsync(context, "AzureAd");

                    if (!result.Succeeded)
                    {
                        await authenticationService.ChallengeAsync(context, "AzureAd", new AuthenticationProperties { RedirectUri = $"{path}" });
                        return;
                    }
                }
                #endregion

                #region testing
                //var path = context.Request.Path;
                //if (path.StartsWithSegments("/azure"))
                //{
                //    var result = await authenticationService.AuthenticateAsync(context, "AzureAd");

                //    if (!result.Succeeded)
                //    {
                //        await authenticationService.ChallengeAsync(context, "AzureAd", new AuthenticationProperties { RedirectUri = $"/test{path}" });
                //        return;
                //    }
                //}
                //if (path.StartsWithSegments("/bcc-platform"))
                //{
                //    var result = await authenticationService.AuthenticateAsync(context, "AzureAd");

                //    if (!result.Succeeded)
                //    {
                //        await authenticationService.ChallengeAsync(context, "AzureAd", new AuthenticationProperties { RedirectUri = $"{path}" });
                //        return;
                //    }
                //}
                #endregion

                await _next(context);
            }
            catch
            {
                context.Response.Redirect("https://developer.bcc.no/");
                await _next(context);
            }
        }
    }
}