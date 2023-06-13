using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using BccCode.DocumentationSite.Services;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Caching.Memory;
using Azure;
using IdentityServer4.Extensions;
using System.Text.RegularExpressions;

namespace BccCode.DocumentationSite.Controllers
{
    [Route("test/azure")]
    [ApiController]
    public class AzureTest : ControllerBase
    {
        public AzureTest()
        {
        }

        [HttpGet]
        public async Task<object> azure()
        {
            Dictionary<string,string> d = new Dictionary<string,string>();
            string result = "";
            foreach (var a in Request.Headers)
            {
                d.Add(a.Key, a.Value);
            }

            foreach (var b in d)
            {
                result += b.Key + "=" + b.Value + "\n";
            }

            return result;
        }
    }
}
