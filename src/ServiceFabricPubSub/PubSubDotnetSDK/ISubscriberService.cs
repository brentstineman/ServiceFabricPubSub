using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PubSubDotnetSDK
{
    /// <summary>
    /// Subscriber service method for internal cluster calls
    /// </summary>
    public interface ISubscriberService : IService
    {
        // TODO : renamed async method with Async suffix

        /// <summary>
        /// Dequeue the first available message. 
        /// If no message available, return null.
        /// </summary>
        /// <returns>PubSubMessage instance or null if queue is empty</returns>
        Task<PubSubMessage> Pop();


        /// <summary>
        /// get the queued messages count. 
        /// </summary>
        /// <returns>count of message in the subscriber queue. 0 if empty</returns>
        Task<long> CountAsync();
    }
}
