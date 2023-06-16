using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BccCode.DocumentationSite.Controllers
{
    [Route("logout/portal")]
    [ApiController]
    public class LogoutPortal : ControllerBase
    {

        [HttpGet]
        public async Task<object> Logout()
        {
            await HttpContext.SignOutAsync("CookiesP");
            await HttpContext.SignOutAsync("Portal");

            Response.Headers.Add("REFRESH", "5;URL=/");
            return "Successfuly loggedout of BCC Portal";
        }
    }
}
