using System;
using System.Fabric;
using System.Fabric.Management.ServiceModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RequestRouterService
{
    public class ReverseProxyPortResolver
    {
        // TODO: not sure if this logic is right. Need to investigate more. 
        // HttpApplicationGatewayEndpoint port changes per node . . . . 

        /// <summary>
        /// Represents the port that the current fabric node is configured
        /// to use when using a reverse proxy on localhost
        /// </summary>
        public async Task<int> GetReverseProxyPortAsync()
        {
            return 19081;
            
            // NOTE: This doesn't seem to be working reliably. Need to revisit.
            /*
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
            */
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