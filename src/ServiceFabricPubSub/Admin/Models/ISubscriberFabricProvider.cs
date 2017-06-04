using System.Collections.Generic;
using System.Fabric;
using System.Threading.Tasks;

namespace Admin.Models
{
    public interface ISubscriberFabricProvider
    {
        Task<IEnumerable<SubscriberInfo>> GetSubscribers(string topic, string applicationName);
    }

    public class SubscriberInfo
    {
        public string ServiceName { get; set; }
        public string ServiceStatus { get; set; }
    }
}