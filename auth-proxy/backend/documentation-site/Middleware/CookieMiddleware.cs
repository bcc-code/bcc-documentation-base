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
                var requestCookies = context.Request.Cookies;

                List<string> cookies = new List<string>();

                foreach (var cookie in requestCookies) 
                {
                    if (cookie.Key.StartsWith(".AspNetCore.Correlation"))
                    {
                        var s = cookie.Key.Substring((".AspNetCore.Correlation").Length);
                        if (s != "" && s != null)
                        {
                            cookies.Add(cookie.Key);
                            context.Response.Cookies.Append(".AspNetCore.Correlation", s, new CookieOptions() { Expires = DateTimeOffset.UtcNow.AddMinutes(15), HttpOnly = true, Secure = true});
                        }
                    }
                    else if(cookie.Key.StartsWith(".AspNetCore.OpenIdConnect.Nonce"))
                    {
                        var s = cookie.Key.Substring((".AspNetCore.OpenIdConnect.Nonce").Length);
                        if (s != "" && s != null)
                        {
                            cookies.Add(cookie.Key);
                            context.Response.Cookies.Append(".AspNetCore.OpenIdConnect.Nonce", s, new CookieOptions() { Expires = DateTimeOffset.UtcNow.AddMinutes(15), HttpOnly = true, Secure = true });
                        }
                    }
                }
                try
                {
                    foreach (var cookie in cookies)
                    {
                        context.Response.Cookies.Delete(cookie);
                    }
                }
                catch
                {
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
