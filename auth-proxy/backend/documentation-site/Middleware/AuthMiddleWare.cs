using Microsoft.AspNetCore.Authentication;

namespace BccCode.DocumentationSite.Middleware
{
    public class AuthMiddleWare
    {
        private readonly RequestDelegate _next;
        private readonly ILogger logger;

        public AuthMiddleWare(RequestDelegate next, ILoggerFactory logger)
        {
            _next = next;
            this.logger = logger.CreateLogger<AuthMiddleWare>();
        }

        public async Task InvokeAsync(HttpContext context, IAuthenticationService authenticationService)
        {
            var path = context.Request.Path;
            try
            {
                if (path.StartsWithSegments("/azure"))
                {
                    var result = await authenticationService.AuthenticateAsync(context, "AzureAd");

                    if (!result.Succeeded)
                    {
                        await authenticationService.ChallengeAsync(context, "AzureAd", new AuthenticationProperties { RedirectUri = $"/test{path}" });
                        return;
                    }
                }
                if (path.StartsWithSegments("/bcc-platform"))
                {
                    var result = await authenticationService.AuthenticateAsync(context, "AzureAd");
                    logger.LogCritical($"Authenticated = {result.Succeeded.ToString()}");

                    if (!result.Succeeded)
                    {
                        logger.LogCritical("Challenging...");
                        await authenticationService.ChallengeAsync(context, "AzureAd", new AuthenticationProperties { RedirectUri = $"{path}" });
                        logger.LogCritical("Challenge completed");
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
            }
            logger.LogCritical("going next");
            await _next(context);
        }
    }
}