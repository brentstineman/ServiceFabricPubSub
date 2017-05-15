using System;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Web.Http;

namespace RequestRouterService.Controllers
{
    public class RequestController : ApiController
    {
        // PUT api/tenantId/topicName
        public async Task<HttpResponseMessage> Put(string tenantId, string topicName)
        {
            HttpResponseMessage responseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            string partitionKey = string.Empty;

            // TODO: Parse the partition key from the body or the input param?
            string messageBody = await this.Request.Content.ReadAsStringAsync();

            bool isAuthenticated = await AuthenticateRequest();

            if (isAuthenticated)
            {
                // Call the Topic Service to post the message.
                var response = await InvokeService("TenantApplication", "TopicService", string.Empty, partitionKey);
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
            }

            return responseMessage;
        }

        private async Task<bool> AuthenticateRequest()
        {
            bool isAuthenticated = false;

            // TODO: Call the real service.


            var response = await InvokeService("FrontEnd", "Administration", string.Empty, string.Empty);
            if (response != null && response.IsSuccessStatusCode)
            {
                isAuthenticated = true;
            }

            return isAuthenticated;
        }

        // GET api/tenantId/topicName
        public HttpResponseMessage Get(string tenantId, string topicName)
        {
            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }

        private static async Task<HttpResponseMessage> InvokeService(string appName, string serviceName, string path, string partitionKey)
        {
            HttpResponseMessage responseMessage;
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    UriBuilder topicServiceUriBuilder = new UriBuilder("http", "localhost", 19008)
                    {
                        Path = $"{appName}/{serviceName}?PartitionKey={partitionKey}&PartitionKind=Int64Range"
                    };

                    responseMessage = await httpClient.GetAsync(topicServiceUriBuilder.Uri);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                responseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    ReasonPhrase = e.Message
                };
            }

            return responseMessage;
        }
    }
}
