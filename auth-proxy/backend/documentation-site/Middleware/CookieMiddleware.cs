namespace BccCode.DocumentationSite.Middleware
{
    public class CookieMiddleware
    {
        private readonly RequestDelegate _next;

        public CookieMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Modify or manipulate cookies here
            // Example: Set a new value for a cookie
            try
            {
                var a = context.Request.Cookies;

                foreach (var cookie in a) 
                {
                    if (cookie.Key.StartsWith(".AspNetCore.Correlation"))
                    {
                        var s = cookie.Key.Substring((".AspNetCore.Correlation").Length);
                        if (s != "" && s != null)
                        {
                            context.Response.Cookies.Append(".AspNetCore.Correlation", s);
                        }
                    }
                    else if(cookie.Key.StartsWith(".AspNetCore.OpenIdConnect.Nonce"))
                    {
                        var s = cookie.Key.Substring((".AspNetCore.OpenIdConnect.Nonce").Length);
                        if (s != "" && s != null)
                        {
                            context.Response.Cookies.Append(".AspNetCore.OpenIdConnect.Nonce", s);
                        }
                    }
                }

                // Call the next middleware in the pipeline
                await _next(context);
            }
            catch
            {
                return;
            }
        }
    }
}
