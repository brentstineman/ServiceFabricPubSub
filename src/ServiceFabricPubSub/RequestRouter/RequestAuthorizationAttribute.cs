using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace RequestRouterService
{
    public class RequestAuthorizationAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (Authorize(actionContext))
            {
                return;
            }

            HandleUnauthorizedRequest(actionContext);
        }

        private bool Authorize(HttpActionContext actionContext)
        {
            bool isAuthorized = false;

            try
            {
                var requestKeyHeaderValue = actionContext.Request.Headers.GetValues("x-request-key").FirstOrDefault();

                if (!string.IsNullOrEmpty(requestKeyHeaderValue))
                {
                    // TODO: perform validation
                    HttpServiceUriBuilder builder = new HttpServiceUriBuilder
                    {
                        ServiceName = "TenantApplication/AdminService"
                    };
                    Uri serviceUri = builder.Build();

                    using (HttpClient httpClient = new HttpClient())
                    {
                        var response = httpClient.GetAsync(serviceUri).Result;
                    }
                }

                isAuthorized = true;

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);

                // TODO: Log this someplace better.

                isAuthorized = false;
            }

            return isAuthorized;
        }
    }
}