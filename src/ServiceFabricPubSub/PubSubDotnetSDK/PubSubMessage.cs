using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace PubSubDotnetSDK
{
    [DataContract]
    public class PubSubMessage
    {
        public PubSubMessage()
        {
            this.Message = null;
        }

        public PubSubMessage(string message)
        {
            this.Message = message;
        }

        [DataMember]
        public string Message { get; set; }
    }
}
