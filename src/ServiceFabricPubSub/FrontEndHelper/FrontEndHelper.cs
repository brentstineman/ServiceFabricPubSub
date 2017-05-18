using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FrontEndHelper
{
    public class FrontEndHelper
    {
        private const string TenantApplicationAppName = "TenantApplication";
        private const string TenantApplicationAdminServiceName = "Admin";
       

        public static async Task<int> GetReverseProxyPortAsync()
        {
            int? reverseProxyPort = null;
            ReverseProxyPortResolver portResolver = new ReverseProxyPortResolver();

            if (reverseProxyPort == null)
            {
                reverseProxyPort = await portResolver.GetReverseProxyPortAsync();
            }
            return reverseProxyPort.Value;
        }

        public static async Task<string> GetAuthKeyAsync(string tenantName, string keyName)
        {
            string keyValue = null;
            HttpResponseMessage keyResponseMessage;

            // TODO: Probably should cache this info someplace. No need to look up every time.
            ReverseProxyPortResolver portResolver = new ReverseProxyPortResolver();
            int reverseProxyPortNumber = await portResolver.GetReverseProxyPortAsync();

            HttpServiceUriBuilder builder = new HttpServiceUriBuilder
            {
                PortNumber = reverseProxyPortNumber,                
                ServiceName = $"{tenantName}/{TenantApplicationAdminServiceName}/api/keys/{keyName}"                
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
