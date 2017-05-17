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
            string key1 = string.Empty;
            string key2 = string.Empty;

            try
            {
                var requestKeyHeaderValue = actionContext.Request.Headers.GetValues("x-request-key").FirstOrDefault();

                string tenantName = GetTenantNameFromUrl(actionContext.Request.RequestUri);

                // TODO: Parse the request URL to get the tenant name. Use the tenant name to call the tenant specific
                //       application's admin service to get the key(s). 
                //       Validate the admin key(s) against the ones from the service.
                if (!string.IsNullOrEmpty(requestKeyHeaderValue))
                {
                    // TODO: perform validation
                    HttpServiceUriBuilder builder = new HttpServiceUriBuilder
                    {
                        //ServiceName = $"{tenantName}/AdminService/api/key1"
                        ServiceName = $"TenantApplication/AdminService/api/key1"
                    };
                    Uri serviceUri = builder.Build();

                    HttpResponseMessage key1ResponseMessage;
                    using (HttpClient httpClient = new HttpClient())
                    {
                        key1ResponseMessage = httpClient.GetAsync(serviceUri).Result;
                    }

                    if (key1ResponseMessage != null && key1ResponseMessage.IsSuccessStatusCode)
                    {
                        key1 = key1ResponseMessage.Content.ReadAsStringAsync().Result;
                    }
                }

                if (key1 == requestKeyHeaderValue)
                {
                    isAuthorized = true;
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

        private string GetTenantNameFromUrl(Uri requestUri)
        {
            if (requestUri == null)
                throw new ArgumentNullException(nameof(requestUri));

            return requestUri.Segments[3].Trim('/');
        }
    }
}