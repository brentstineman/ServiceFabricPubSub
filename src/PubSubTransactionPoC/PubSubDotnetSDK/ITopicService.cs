using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.ServiceFabric.Services.Remoting;
using System.Threading.Tasks;

namespace PubSubDotnetSDK
{
    public interface ITopicService : IService
    {
        /// <summary>
        /// Push a new message in the topic
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        Task Push(PubSubMessage msg);

        /// <summary>
        /// Just for testing in sprint0
        /// </summary>
        /// <returns></returns>
        Task<PubSubMessage> InternalPop();
    }
}
