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
        /// Use to register a new (or restarted subscriber)
        /// </summary>
        /// <param name="subscriberId"></param>
        /// <returns></returns>
        Task RegisterSubscriber(string subscriberId);

        

        /// <summary>
        /// Just for testing in sprint0
        /// </summary>
        /// <param name="subcriberId">Name the of caller subscriber service</param>
        /// <returns></returns>
        Task<PubSubMessage> InternalPop(string subscriberId);
    }
}
