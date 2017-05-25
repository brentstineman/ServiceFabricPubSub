using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Fabric.Query;
using System.Fabric;

namespace Admin.Models
{
    public class SubscriberFabricProvider: ISubscriberFabricProvider
    {
        FabricClient fabric;

        public SubscriberFabricProvider():this(new FabricClient()) { }

        public SubscriberFabricProvider(FabricClient fabric)
        {
            this.fabric = fabric;
        }

        public async Task<IEnumerable<SubscriberInfo>> GetSubscribers(string topic, string applicationName)
        {
            ServiceList services = await fabric.QueryManager.GetServiceListAsync(new Uri(applicationName));

            return services.
                Where(x => x.ServiceTypeName == Constants.SUBSCRIBER_SERVICE_TYPE_NAME && x.ServiceName.IsTopic(topic))
                    .Select(x => new SubscriberInfo
                    {
                        ServiceName = x.ServiceName.ToString(),
                        ServiceStatus = x.ServiceStatus.ToString()
                    });
        }
    }
}
