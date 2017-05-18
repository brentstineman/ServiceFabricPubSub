using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using FrontEndHelper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RequestRouterService.Controllers
{
    [RequestAuthorization]
    public class RequestController : ApiController
    {
        private const string TenantApplicationAdminServiceName = "Admin";
        private const string TenantApplicationTopicServiceName = "topics";

        private static int _reverseProxyPort;

        // POST api/tenantId/topicName
        public async Task<HttpResponseMessage> Post(string tenantId, string topicName)
        {
            HttpResponseMessage responseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            // Assuming the message body will contain the content for the data to put to the topic.
            string messageBody = await this.Request.Content.ReadAsStringAsync();

            HttpServiceUriBuilder builder = new HttpServiceUriBuilder()
            {
                PortNumber = await GetReverseProxyPortAsync(),
                ServiceName = $"{tenantId}/{TenantApplicationTopicServiceName}/{topicName}/api/"
            };

            HttpResponseMessage topicResponseMessage;
            using (HttpClient httpClient = new HttpClient())
            {
                HttpContent postContent = new StringContent(messageBody);
                postContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                topicResponseMessage = await httpClient.PostAsync(builder.Build(), postContent);
            }

            if (topicResponseMessage != null && topicResponseMessage.IsSuccessStatusCode)
            {
                responseMessage.StatusCode = HttpStatusCode.Accepted;

                var msg = await topicResponseMessage.Content.ReadAsStringAsync();
                
                System.Diagnostics.Debug.WriteLine($"Received response of '{msg}'.");
            }
            else
            {
                responseMessage.StatusCode = topicResponseMessage?.StatusCode ?? HttpStatusCode.InternalServerError;
                responseMessage.ReasonPhrase = topicResponseMessage?.ReasonPhrase ?? "Internal error";
            }

            return responseMessage;
        }

        // GET api/tenantId/topicName/subscriber
        public async Task<HttpResponseMessage> Get(string tenantId, string topicName, string subscriberName)
        {
            HttpResponseMessage responseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            HttpServiceUriBuilder builder = new HttpServiceUriBuilder()
            {
                PortNumber = await GetReverseProxyPortAsync(),
                ServiceName = $"{tenantId}/{TenantApplicationTopicServiceName}/{topicName}/{subscriberName}/api"
            };
            Uri serviceUri = builder.Build();

            HttpResponseMessage topicResponseMessage;
            using (HttpClient httpClient = new HttpClient())
            {
                topicResponseMessage = await httpClient.GetAsync(serviceUri);
            }

            if (topicResponseMessage != null && topicResponseMessage.IsSuccessStatusCode)
            {
                responseMessage.StatusCode = HttpStatusCode.OK;

                var msg = await topicResponseMessage.Content.ReadAsStringAsync();

                HttpContent responseContent = new StringContent(msg, Encoding.UTF8, "application/json");
                responseMessage.Content = responseContent;
            }
            else
            {
                responseMessage.StatusCode = topicResponseMessage?.StatusCode ?? HttpStatusCode.InternalServerError;
                responseMessage.ReasonPhrase = topicResponseMessage?.ReasonPhrase ?? "Internal error";
            }

            return responseMessage;
        }

        // GET api/tenantId
        public async Task<HttpResponseMessage> GetTopics(string tenantId)
        {
            HttpResponseMessage responseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            HttpServiceUriBuilder builder = new HttpServiceUriBuilder()
            {
                PortNumber = await GetReverseProxyPortAsync(),
                ServiceName = $"{tenantId}/{TenantApplicationAdminServiceName}/api/topics/"
            };

            HttpResponseMessage topicResponseMessage;
            using (HttpClient httpClient = new HttpClient())
            {
                topicResponseMessage = await httpClient.GetAsync(builder.Build());
            }

            IList<string> topicNameList = new List<string>();
            if (topicResponseMessage != null && topicResponseMessage.IsSuccessStatusCode)
            {
                var msg = await topicResponseMessage.Content.ReadAsStringAsync();

                dynamic x = JArray.Parse(msg);
                foreach (dynamic node in x)
                {
                    string serviceName = node.serviceName;
                    serviceName = serviceName.Split('/').LastOrDefault();
                    topicNameList.Add(serviceName);
                }

                string topicNamesJson = JsonConvert.SerializeObject(topicNameList,
                    new JsonSerializerSettings
                    {
                        Formatting = Formatting.Indented
                    });
                responseMessage.Content = new StringContent(topicNamesJson);
                responseMessage.StatusCode = HttpStatusCode.OK;
            }
            else
            {
                responseMessage.StatusCode = HttpStatusCode.InternalServerError;
                responseMessage.ReasonPhrase = topicResponseMessage?.ReasonPhrase ?? "Internal error";
            }

            return responseMessage;
        }

        private static async Task<int> GetReverseProxyPortAsync()
        {
            ReverseProxyPortResolver portResolver = new ReverseProxyPortResolver();

            if (_reverseProxyPort == 0)
            {
                _reverseProxyPort = await portResolver.GetReverseProxyPortAsync();
            }

            return _reverseProxyPort;
        }
    }
}
