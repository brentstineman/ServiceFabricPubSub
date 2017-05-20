using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RequestRouterService.Controllers
{
    //TODO: add authz (HACK for testing)
    //[RequestAuthorization]
    public class RequestController : ApiController
    {
        private const string TenantApplicationAdminServiceName = "Admin";
        private const string TenantApplicationTopicServiceName = "topics";

        private static int? reverseProxyPort = null;

        // POST api/tenantId/topicName
        public async Task<HttpResponseMessage> Post(string tenantId, string topicName, string message)
        {
            HttpResponseMessage responseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            // Assuming the message body will contain the content for the data to put to the topic.
            //string messageBody = await this.Request.Content.ReadAsStringAsync();

            // TODO: Clean this up everywhere . . . . 
            await GetReverseProxyPortAsync();

            HttpServiceUriBuilder builder = new HttpServiceUriBuilder()
            {
                // TODO: Need to use the tenant name instead of 'TenantApplication'.

                PortNumber = reverseProxyPort.Value,
                ServiceName = $"{tenantId}/{TenantApplicationTopicServiceName}/api/topics/{topicName}"
            };

            HttpResponseMessage topicResponseMessage;
            using (HttpClient httpClient = new HttpClient())
            {
                HttpContent postContent = new StringContent(message);
                postContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                topicResponseMessage = await httpClient.PostAsync(builder.Build(), postContent);
            }

            if (topicResponseMessage != null && topicResponseMessage.IsSuccessStatusCode)
            {
                responseMessage.StatusCode = HttpStatusCode.Accepted;

                var msg = await topicResponseMessage.Content.ReadAsStringAsync();

                // TODO: do something with the response.
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

            await GetReverseProxyPortAsync();

            HttpServiceUriBuilder builder = new HttpServiceUriBuilder()
            {
                PortNumber = reverseProxyPort.Value,
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
        //public async Task<HttpResponseMessage> GetTopics(string tenantId)
        //{
        //    HttpResponseMessage responseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        //    await GetReverseProxyPortAsync();

        //    HttpServiceUriBuilder builder = new HttpServiceUriBuilder()
        //    {
        //        PortNumber = reverseProxyPort.Value,
        //        ServiceName = $"{tenantId}/{TenantApplicationAdminServiceName}/api/topics/"
        //    };

        //    HttpResponseMessage topicResponseMessage;
        //    using (HttpClient httpClient = new HttpClient())
        //    {
        //        topicResponseMessage = await httpClient.GetAsync(builder.Build());
        //    }

        //    IList<string> topicNameList = new List<string>();
        //    if (topicResponseMessage != null && topicResponseMessage.IsSuccessStatusCode)
        //    {
        //        var msg = await topicResponseMessage.Content.ReadAsStringAsync();

        //        dynamic x = JArray.Parse(msg);
        //        foreach (dynamic node in x)
        //        {
        //            string serviceName = node.serviceName;
        //            serviceName = serviceName.Split('/').LastOrDefault();
        //            topicNameList.Add(serviceName);
        //        }

        //        string topicNamesJson = JsonConvert.SerializeObject(topicNameList,
        //            new JsonSerializerSettings
        //            {
        //                Formatting = Formatting.Indented
        //            });
        //        responseMessage.Content = new StringContent(topicNamesJson);
        //        responseMessage.StatusCode = HttpStatusCode.OK;
        //    }
        //    else
        //    {
        //        responseMessage.StatusCode = HttpStatusCode.InternalServerError;
        //        responseMessage.ReasonPhrase = topicResponseMessage?.ReasonPhrase ?? "Internal error";
        //    }

        //    return responseMessage;
        //}

        private static async Task GetReverseProxyPortAsync()
        {
            ReverseProxyPortResolver portResolver = new ReverseProxyPortResolver();

            if (reverseProxyPort == null)
            {
                reverseProxyPort = await portResolver.GetReverseProxyPortAsync();
            }
        }
    }
}
