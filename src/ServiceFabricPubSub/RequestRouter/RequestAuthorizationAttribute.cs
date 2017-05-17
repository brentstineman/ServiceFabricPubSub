using System;
using System.Fabric;
using System.Fabric.Management.ServiceModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Xml.Serialization;

namespace RequestRouterService
{
    public class RequestAuthorizationAttribute : AuthorizeAttribute
    {
        private const string TenantApplicationAppName = "PubSubTransactionPoC";
        private const string TenantApplicationAdminServiceName = "Admin";

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (Authorize(actionContext))
            {
                return;
            }

            HandleUnauthorizedRequest(actionContext);
        }

        private bool Authorize(HttpActionContext actionContext)
        {
            bool isAuthorized = false;
            string key1 = string.Empty;
            string key2 = string.Empty;

            try
            {
                var requestKeyHeaderValue = actionContext.Request.Headers.GetValues("x-request-key").FirstOrDefault();

                string tenantName = GetTenantNameFromUrl(actionContext.Request.RequestUri);

                // Parse the request URL to get the tenant name. Use the tenant name to call the tenant specific
                // application's admin service to get the key(s). 
                // Validate the admin key(s) against the ones from the service.
                if (!string.IsNullOrEmpty(requestKeyHeaderValue))
                {
                    // TODO: Dynamically get the reverse proxy port number (seems to be different locally vs. cloud).

                    // TODO: Make these async too.
                    key1 = GetAuthKey1(tenantName).Result;
                    key2 = GetAuthKey2(tenantName).Result;
                }

                if (key1 == requestKeyHeaderValue || key2 == requestKeyHeaderValue)
                {
                    isAuthorized = true;
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);

                // TODO: Log this someplace better.
            }

            return isAuthorized;
        }

        private async Task<string> GetAuthKey1(string tenantName)
        {
            return await GetAuthKeyAsync(tenantName, "key1");
        }

        private async Task<string> GetAuthKey2(string tenantName)
        {
            return await GetAuthKeyAsync(tenantName, "key2");
        }

        private async Task<string> GetAuthKeyAsync(string tenantName, string keyName)
        {
            string keyValue = null;
            HttpResponseMessage keyResponseMessage;
            ReverseProxyPortResolver portResolver = new ReverseProxyPortResolver();

            int reverseProxyPortNumber = 19081; //await portResolver.GetReverseProxyPortAsync();

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

        private string GetTenantNameFromUrl(Uri requestUri)
        {
            if (requestUri == null)
                throw new ArgumentNullException(nameof(requestUri));

            return requestUri.Segments[3].Trim('/');
        }
    }

    public class ReverseProxyPortResolver
    {
        /// <summary>
        /// Represents the port that the current fabric node is configured
        /// to use when using a reverse proxy on localhost
        /// </summary>
        public async Task<int> GetReverseProxyPortAsync()
        {
            ClusterManifestType deserializedManifest;
            using (var cl = new FabricClient())
            {
                var manifestStr = await cl.ClusterManager.GetClusterManifestAsync().ConfigureAwait(false);
                var serializer = new XmlSerializer(typeof(ClusterManifestType));

                using (var reader = new StringReader(manifestStr))
                {
                    deserializedManifest = (ClusterManifestType)serializer.Deserialize(reader);
                }
            }

            //Fetch the setting from the correct node type
            var nodeType = await GetNodeTypeAsync();
            var nodeTypeSettings = deserializedManifest.NodeTypes.Single(x => x.Name.Equals(nodeType));
            return int.Parse(nodeTypeSettings.Endpoints.HttpApplicationGatewayEndpoint.Port);
        }

        private async Task<string> GetNodeTypeAsync()
        {
            try
            {
                CancellationToken ct = CancellationToken.None;
                var nodeContext = await FabricRuntime.GetNodeContextAsync(TimeSpan.FromSeconds(3), ct);
                return nodeContext.NodeType;
            }
            catch (FabricConnectionDeniedException)
            {
                //this code was invoked from a non-fabric started application
                //likely a unit test
                return "NodeType0";
            }

        }
    }
}