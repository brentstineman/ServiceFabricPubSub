using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Services.Remoting;

namespace AdminService
{
    public interface IAdminService : IService
    {
        Task<string> GetKey1();
        Task<string> GetKey2();
        Task CreateNewTopic(string topicName);
        Task DeleteTopic(string topicName);
        Task RegenerateKeys();
    }
}