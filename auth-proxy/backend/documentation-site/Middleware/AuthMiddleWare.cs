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
        private readonly ILogger _logger;

        public AuthMiddleWare(RequestDelegate next, IMemoryCache cache, IConfiguration config, ILoggerFactory logger)
        {
            _next = next;
            _cache = cache;
            _config = config;
            _logger = logger.CreateLogger<AuthMiddleWare>();
        }

        public async Task InvokeAsync(HttpContext context, IAuthenticationService authenticationService)
        {
            try
            {
                #region a
                _logger.LogCritical("setting up variables");
                var path = context.Request.Path;
                //Extract container name from the path which appears after the first '/' in the path
                var containerName = path.Value!.Split('/')[1];
                var credential = new DefaultAzureCredential();
                var envVar = new EnviromentVar(_config);
                ContainerService cService = new ContainerService(credential, envVar.GetEnviromentVariable("StorageUrl"), _cache);

                _logger.LogCritical("authenticating...");
                var authmethod = await cService.AuthProvider(containerName);
                _logger.LogCritical($"auth method = {authmethod} path = {path}");
                if (authmethod == "azuread")
                {
                    var result = await authenticationService.AuthenticateAsync(context, "AzureAd");
                    _logger.LogCritical($"is authenticated = {result.Succeeded.ToString()}");
                    if (!result.Succeeded)
                    {
                        await authenticationService.ChallengeAsync(context, "AzureAd", new AuthenticationProperties { RedirectUri = $"{path}" });
                        _logger.LogCritical($"completed challenge to path = {path}");
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
                _logger.LogCritical("calling next");
                await _next(context);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                //await _next(context);
                return;
            }
        }
    }
}