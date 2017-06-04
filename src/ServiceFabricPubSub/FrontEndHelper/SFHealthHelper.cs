using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Health;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FrontEndHelper
{
    public class SFHealthHelper
    {
        private static Uri ApplicationName = new Uri("fabric:/FrontEnd");
        private static string ServiceManifestName = "Administration";
        private static string NodeName = FabricRuntime.GetNodeContext().NodeName;        
        private static FabricClient Client = new FabricClient(new FabricClientSettings() { HealthReportSendInterval = TimeSpan.FromSeconds(0) });

        public static void SendReport(bool good,string component)
        {
            // Test whether the resource can be accessed from the node
            HealthState healthState = good ? HealthState.Ok : HealthState.Warning;

            // Send report on deployed service package, as the connectivity is needed by the specific service manifest
            // and can be different on different nodes
            var deployedServicePackageHealthReport = new DeployedServicePackageHealthReport(
                ApplicationName,
                ServiceManifestName,
                NodeName,
                new HealthInformation("SFPubSubAdmin", component, healthState));

            // TODO: handle exception. Code omitted for snippet brevity.
            // Possible exceptions: FabricException with error codes
            // FabricHealthStaleReport (non-retryable, the report is already queued on the health client),
            // FabricHealthMaxReportsReached (retryable; user should retry with exponential delay until the report is accepted).
            Client.HealthManager.ReportHealth(deployedServicePackageHealthReport);
        }
    }
}
