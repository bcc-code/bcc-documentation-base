using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BccCode.DocumentationSite.Controllers
{
    [Route("logout/azure")]
    [ApiController]
    public class LogoutAzure : ControllerBase
    {

        [HttpGet]
        public async Task<object> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            await HttpContext.SignOutAsync("AzureAd");

            Response.Headers.Add("REFRESH", "5;URL=/");
            return "Successfuly loggedout of AzureAD";
        }
    }
}
