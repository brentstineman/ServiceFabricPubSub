using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PubSubDotnetSDK
{
    public interface ISubscriberService : IService
    {
        Task<PubSubMessage> Pop();
    }
}
