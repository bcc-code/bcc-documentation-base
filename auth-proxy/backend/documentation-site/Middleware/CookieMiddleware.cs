namespace BccCode.DocumentationSite.Middleware
{
    public class CookieMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public CookieMiddleware(RequestDelegate next, ILoggerFactory logger)
        {
            _next = next;
            _logger = logger.CreateLogger<CookieMiddleware>();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Modify or manipulate cookies here
            // Example: Set a new value for a cookie
            try
            {
                _logger.LogCritical("requesting cookies");
                var requestCookies = context.Request.Cookies;

                List<string> cookies = new List<string>();

                _logger.LogCritical("starting cookiefix");
                foreach (var cookie in requestCookies) 
                {
                    _logger.LogCritical($"proccessing cookie {cookie.Key}");

                    if (cookie.Key.StartsWith(".AspNetCore.Correlation"))
                    {
                        var s = cookie.Key.Substring((".AspNetCore.Correlation").Length);
                        _logger.LogCritical($"cookie value = {s}");
                        if (s != "" && s != null)
                        {
                            _logger.LogCritical($"setting new cookie");
                            cookies.Add(cookie.Key);
                            context.Response.Cookies.Append(".AspNetCore.Correlation", s, new CookieOptions() { Expires = DateTimeOffset.Now.AddMinutes(15), HttpOnly = true, Secure = true});
                        }
                    }
                    else if(cookie.Key.StartsWith(".AspNetCore.OpenIdConnect.Nonce"))
                    {
                        var s = cookie.Key.Substring((".AspNetCore.OpenIdConnect.Nonce").Length);
                        _logger.LogCritical($"cookie value = {s}");
                        if (s != "" && s != null)
                        {
                            _logger.LogCritical($"setting new cookie");
                            cookies.Add(cookie.Key);
                            context.Response.Cookies.Append(".AspNetCore.OpenIdConnect.Nonce", s, new CookieOptions() { Expires = DateTimeOffset.Now.AddMinutes(15), HttpOnly = true, Secure = true });
                        }
                    }
                }
                try
                {
                    _logger.LogCritical("deleting null cookies");
                    foreach (var cookie in cookies)
                    {
                        _logger.LogCritical($"deleting cookie {cookie}");
                        context.Response.Cookies.Delete(cookie);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError("cookie deleting error!!! = " + e.Message);
                }

                // Call the next middleware in the pipeline
                await _next(context);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                context.Response.Redirect("https://developer.bcc.no");
                return;
            }
        }
    }
}
