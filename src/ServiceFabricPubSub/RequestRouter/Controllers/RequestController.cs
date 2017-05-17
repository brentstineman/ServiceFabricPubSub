using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace RequestRouterService.Controllers
{
    [RequestAuthorization]
    public class RequestController : ApiController
    {
        private const string TenantApplicationAdminServiceName = "Admin";

        // PUT api/tenantId/topicName
        public async Task<HttpResponseMessage> Put(string tenantId, string topicName)
        {
            HttpResponseMessage responseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            // Assuming the message body will contain the content for the data to put to the topic.
            string messageBody = await this.Request.Content.ReadAsStringAsync();

            ReverseProxyPortResolver portResolver = new ReverseProxyPortResolver();
            int portNumber = await portResolver.GetReverseProxyPortAsync();

            HttpServiceUriBuilder builder = new HttpServiceUriBuilder()
            {
                // TODO: Need to use the tenant name instead of 'TenantApplication'.

                // TODO: Get port number dynamically
                PortNumber = 19081,
                //ServiceName = $"{tenantName}/{TenantApplicationAdminServiceName}/api/topics/topicName"
                ServiceName = $"TenantApplication/{TenantApplicationAdminServiceName}/api/topics/{topicName}"
            };

            // Call the Topic Service to post the message.
            HttpResponseMessage topicResponseMessage;
            using (HttpClient httpClient = new HttpClient())
            {
                topicResponseMessage = await httpClient.PutAsync(builder.Build(), null);
            }

            if (topicResponseMessage != null && topicResponseMessage.IsSuccessStatusCode)
            {
                responseMessage.StatusCode = HttpStatusCode.Accepted;

                var msg = await topicResponseMessage.Content.ReadAsStringAsync();

                // TODO: do something with the response.
            }
            else
            {
                responseMessage.StatusCode = HttpStatusCode.InternalServerError;
                responseMessage.ReasonPhrase = topicResponseMessage?.ReasonPhrase ?? "Internal error";
            }

            return responseMessage;
        }

        // GET api/tenantId/topicName
        public HttpResponseMessage Get(string tenantId, string topicName)
        {
            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }
    }
}
