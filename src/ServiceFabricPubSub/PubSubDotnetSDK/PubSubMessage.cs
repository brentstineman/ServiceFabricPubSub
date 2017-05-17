using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace PubSubDotnetSDK
{
    /// <summary>
    /// Internal class used to persist message in ServiceFabric queue
    /// </summary>
    [DataContract]
    public class PubSubMessage
    {
        public PubSubMessage()
        {
            this.Message = null;
        }


        /// <summary>
        /// unique Message ID set by the TopicService when message is received.
        /// used to check duplicate in subscriber.
        /// </summary>
        [DataMember]
        public Guid MessageID { get; set; } 

        /// <summary>
        /// Message content/payload
        /// </summary>
        [DataMember]
        public string Message { get; set; }
    }
}
