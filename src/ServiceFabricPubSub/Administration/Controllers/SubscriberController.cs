using FrontEndHelper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Administration.Controllers
{
    //[RequestAuthorizationAttribute]
    [ServiceRequestActionFilter]
    public class SubscriberController : ApiController
    {
        private const string TenantApplicationAppName = "TenantApplication";
        private const string TenantApplicationAdminServiceName = "Admin";

        [HttpGet()]
        public async Task<HttpResponseMessage> GetSubscribers(string tenantId, string topicName)
        {
            HttpResponseMessage responseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            int reverseProxyPort = await FrontEndHelper.FrontEndHelper.GetReverseProxyPortAsync();

            HttpServiceUriBuilder builder = new HttpServiceUriBuilder()
            {
                PortNumber = reverseProxyPort,

                ServiceName = $"{tenantId}/{TenantApplicationAdminServiceName}/api/{topicName}/subscribers"
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


        [HttpPut()]
        public async Task<HttpResponseMessage> AddSubscriber(string tenantId, string topicName, string subscriberName)
        {
            HttpResponseMessage responseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            int reverseProxyPort = await FrontEndHelper.FrontEndHelper.GetReverseProxyPortAsync();

            HttpServiceUriBuilder builder = new HttpServiceUriBuilder()
            {
                PortNumber = reverseProxyPort,
                ServiceName = $"{tenantId}/{TenantApplicationAdminServiceName}/api/{topicName}/subscribers/{subscriberName}"
            };

            HttpResponseMessage topicResponseMessage;
            using (HttpClient httpClient = new HttpClient())
            {
                topicResponseMessage = await httpClient.PutAsync(builder.Build(),null);
            }

            if (topicResponseMessage != null && topicResponseMessage.IsSuccessStatusCode)
            {
                responseMessage.StatusCode = HttpStatusCode.OK;
            }
            else
            {
                responseMessage.StatusCode = HttpStatusCode.InternalServerError;
                responseMessage.ReasonPhrase = topicResponseMessage?.ReasonPhrase ?? "Internal error";
            }

            return responseMessage;
        }

        [HttpDelete()]
        public async Task<HttpResponseMessage> DeleteSubscriber(string tenantId, string topicName, string subscriberName)
        {
            HttpResponseMessage responseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            int reverseProxyPort = await FrontEndHelper.FrontEndHelper.GetReverseProxyPortAsync();

            HttpServiceUriBuilder builder = new HttpServiceUriBuilder()
            {
                PortNumber = reverseProxyPort,
                ServiceName = $"{tenantId}/{TenantApplicationAdminServiceName}/api/{topicName}/subscribers/{subscriberName}"
            };

            HttpResponseMessage topicResponseMessage;
            using (HttpClient httpClient = new HttpClient())
            {
                topicResponseMessage = await httpClient.DeleteAsync(builder.Build());
            }

            if (topicResponseMessage != null && topicResponseMessage.IsSuccessStatusCode)
            {
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
