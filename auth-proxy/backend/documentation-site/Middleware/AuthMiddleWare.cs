using Microsoft.AspNetCore.Authentication;

namespace BccCode.DocumentationSite.Middleware
{
    public class AuthMiddleWare
    {
        private readonly RequestDelegate _next;

        public AuthMiddleWare(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IAuthenticationService authenticationService)
        {
            var path = context.Request.Path;
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

                if (!result.Succeeded)
                {
                    await authenticationService.ChallengeAsync(context, "AzureAd", new AuthenticationProperties { RedirectUri = $"{path}" });
                    return;
                }
            }

            await _next(context);
        }
    }
}