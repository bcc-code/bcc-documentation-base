using System.Net;
using System.Runtime.CompilerServices;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace BccCode.DocumentationSite.Filters
{
    public class GitAuthAttribute : AuthorizationFilterAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            Console.WriteLine("Onauth called");
            if (!actionContext.Request.Headers.Contains("bla") || actionContext.Request.Headers.GetValues("bla") == null)
            {
                //actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.BadRequest);
                actionContext.Response = new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent("hi") };
            }
        }
        public override Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            Console.WriteLine("async Onauth called");
            if (!actionContext.Request.Headers.Contains("bla") || actionContext.Request.Headers.GetValues("bla") == null)
            {
                //actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.BadRequest);
                actionContext.Response = new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent("hi") };
            }
            return base.OnAuthorizationAsync(actionContext, cancellationToken);
        }
    }
}
