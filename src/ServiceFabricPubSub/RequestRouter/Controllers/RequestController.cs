using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

                PortNumber = portNumber,
                //ServiceName = $"{tenantName}/{TenantApplicationAdminServiceName}/api/topics/topicName"
                ServiceName = $"TenantApplication/{TenantApplicationAdminServiceName}/api/topics/{topicName}"
            };

            // TODO: Call the Topic Service to post the message.
            //HttpResponseMessage topicResponseMessage;
            //using (HttpClient httpClient = new HttpClient())
            //{
            //    topicResponseMessage = await httpClient.PutAsync(builder.Build(), null);
            //}

            //if (topicResponseMessage != null && topicResponseMessage.IsSuccessStatusCode)
            //{
            //    responseMessage.StatusCode = HttpStatusCode.Accepted;

            //    var msg = await topicResponseMessage.Content.ReadAsStringAsync();

            //    // TODO: do something with the response.
            //}
            //else
            //{
            //    responseMessage.StatusCode = HttpStatusCode.InternalServerError;
            //    responseMessage.ReasonPhrase = topicResponseMessage?.ReasonPhrase ?? "Internal error";
            //}

            return responseMessage;
        }

        // GET api/tenantId/topicName
        public HttpResponseMessage Get(string tenantId, string topicName)
        {
            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }

        // GET api/tenantId
        public async Task<HttpResponseMessage> GetTopics(string tenantId)
        {
            HttpResponseMessage responseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            ReverseProxyPortResolver portResolver = new ReverseProxyPortResolver();
            int portNumber = await portResolver.GetReverseProxyPortAsync();

            HttpServiceUriBuilder builder = new HttpServiceUriBuilder()
            {
                // TODO: Need to use the tenant name instead of 'TenantApplication'.

                PortNumber = portNumber,
                //ServiceName = $"{tenantId}/{TenantApplicationAdminServiceName}/api/topics/topicName"
                ServiceName = $"TenantApplication/{TenantApplicationAdminServiceName}/api/topics/"
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
    }
}
