using Microsoft.AspNetCore.Hosting;
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
    [ServiceRequestActionFilter]
    public class TopicsController : ApiController
    {

        private const string TenantApplicationAppName = "TenantApplication";
        private const string TenantApplicationAdminServiceName = "Admin";

        [HttpPut()]
        public async Task<string> CreateTopic(string TenantName, string TopicName)
        {

            var client = new HttpClient();
            
            Uri uri = new Uri("http://localhost:19081/TenantApplication/Admin/api/topics");
            HttpResponseMessage response = await client.PutAsJsonAsync(uri, TopicName);
            
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();

        }

        private async Task<string> GetAuthKeyAsync(string tenantName, string keyName)
        {
            string keyValue = null;
            HttpResponseMessage keyResponseMessage;
            ReverseProxyPortResolver portResolver = new ReverseProxyPortResolver();

            int reverseProxyPortNumber = await portResolver.GetReverseProxyPortAsync();

            HttpServiceUriBuilder builder = new HttpServiceUriBuilder
            {
                PortNumber = reverseProxyPortNumber,
                // http://localhost:19081/PubSubTransactionPoC/Admin/api/keys/key1
                //ServiceName = $"{tenantName}/{TenantApplicationAdminServiceName}/api/keys/key1"
                ServiceName = $"{TenantApplicationAppName}/{TenantApplicationAdminServiceName}/api/keys/{keyName}"
            };
            Uri serviceUri = builder.Build();

            using (HttpClient httpClient = new HttpClient())
            {
                keyResponseMessage = httpClient.GetAsync(serviceUri).Result;
            }

            if (keyResponseMessage != null && keyResponseMessage.IsSuccessStatusCode)
            {
                keyValue = keyResponseMessage.Content.ReadAsStringAsync().Result;
            }

            return keyValue;
        }
    }
}
