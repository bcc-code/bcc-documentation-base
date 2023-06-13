using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BccCode.DocumentationSite.Controllers
{
    [Route("test")]
    [ApiController]
    public class test : ControllerBase
    {

        [HttpGet]
        public async Task<object> Test()
        {
            return "success";
        }
    }
}
