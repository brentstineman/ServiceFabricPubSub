using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.ServiceFabric.Services.Remoting;
using System.Threading.Tasks;

namespace PubSubDotnetSDK
{
    public interface ITopicService : IService
    {
        Task Push(IMessage msg);


    }
}
