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
        /// peek the message (not removed from the subscriver output queue) 
        /// </summary>
        /// <param name="subscriberId"></param>
        /// <returns></returns>
        Task<PubSubMessage> InternalPeek(string subscriberId);



        /// <summary>
        /// dequeue the message and remove it from the queue (to be use by subscriber service)
        /// </summary>
        /// <param name="subcriberId">Name the of caller subscriber service</param>
        /// <returns>message, null if none available</returns>
        Task<PubSubMessage> InternalDequeue(string subscriberId);




    }
}
