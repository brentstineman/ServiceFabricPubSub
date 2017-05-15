using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace PubSubDotnetSDK
{
    [DataContract]
    public class PubSubMessage
    {
        [DataMember]
        public string Message { get; set; }
    }
}
