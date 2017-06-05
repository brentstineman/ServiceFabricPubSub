using FrontEndHelper;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RequestRouterService;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Fabric.Query;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Administration.Controllers
{
    [RequestAuthorizationAttribute]
    [ServiceRequestActionFilter]
    public class TopicsController : ApiController
    {

        private const string TenantApplicationAppName = "TenantApplication";
        private const string TenantApplicationAdminServiceName = "Admin";

        [HttpPut()]
        public async Task<string> CreateTopic(string tenantId, string TopicName)
        {
            //var key = HttpContext.Current.Request.Headers.GetValues("x-request-key").FirstOrDefault();

            HttpResponseMessage responseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            int reverseProxyPort = await FrontEndHelper.FrontEndHelper.GetReverseProxyPortAsync();

            HttpServiceUriBuilder builder = new HttpServiceUriBuilder()
            {
                PortNumber = reverseProxyPort,
                ServiceName = $"{tenantId}/{TenantApplicationAdminServiceName}/api/topics/" + TopicName
            };

            HttpResponseMessage topicResponseMessage;
            using (HttpClient httpClient = new HttpClient())
            {
                topicResponseMessage = await httpClient.PutAsync(builder.Build(),null);
            }

            return "created";

        }

        [HttpDelete()]
        public async Task<string> DeleteTopic(string tenantId, string TopicName)
        {

            HttpResponseMessage responseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            int reverseProxyPort = await FrontEndHelper.FrontEndHelper.GetReverseProxyPortAsync();

            HttpServiceUriBuilder builder = new HttpServiceUriBuilder()
            {
                PortNumber = reverseProxyPort,
                ServiceName = $"{tenantId}/{TenantApplicationAdminServiceName}/api/topics/" + TopicName
            };

            HttpResponseMessage topicResponseMessage;
            using (HttpClient httpClient = new HttpClient())
            {
                topicResponseMessage = await httpClient.DeleteAsync(builder.Build());
            }

            return "deleted";

        }

        [HttpGet()]
        public async Task<HttpResponseMessage> GetTopics(string tenantId)
        {
            HttpResponseMessage responseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            int reverseProxyPort = await FrontEndHelper.FrontEndHelper.GetReverseProxyPortAsync();

            HttpServiceUriBuilder builder = new HttpServiceUriBuilder()
            {
                PortNumber = reverseProxyPort,
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


    }
}
