using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace RequestRouterService.Controllers
{
    [RequestAuthorization]
    public class RequestController : ApiController
    {
        // PUT api/tenantId/topicName
        public async Task<HttpResponseMessage> Put(string tenantId, string topicName)
        {
            HttpResponseMessage responseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            // TODO: Check if the tenant is valid. (TenantApplication/tenantId)
            HttpServiceUriBuilder adminServiceUriBuilder = new HttpServiceUriBuilder()
            {
                ServiceName = $"TenantApplication/{tenantId}",
                ServicePathAndQuery = ""
            };

            HttpResponseMessage adminServiceResponseMessage;
            using (HttpClient httpClient = new HttpClient())
            {
                adminServiceResponseMessage = await httpClient.GetAsync(adminServiceUriBuilder.Build());
            }

            // Assuming the message body will contain the content for the data to put to the topic.
            string messageBody = await this.Request.Content.ReadAsStringAsync();

            HttpServiceUriBuilder builder = new HttpServiceUriBuilder()
            {
                ServiceName = "TenantApplication/TopicSerivce"
            };

            // Call the Topic Service to post the message.
            HttpResponseMessage response;
            using (HttpClient httpClient = new HttpClient())
            {
                 response = await httpClient.GetAsync(builder.Build());
            }

            if (response != null && response.IsSuccessStatusCode)
            {
                responseMessage.StatusCode = HttpStatusCode.Accepted;

                var msg = await response.Content.ReadAsStringAsync();

                // TODO: do something with the response.
            }
            else
            {
                responseMessage.StatusCode = HttpStatusCode.InternalServerError;
                responseMessage.ReasonPhrase = response?.ReasonPhrase ?? "Internal error";
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
