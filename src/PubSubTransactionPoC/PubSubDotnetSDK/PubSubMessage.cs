using System;
using System.Collections.Generic;
using System.Text;

namespace PubSubDotnetSDK
{
    public class PubSubMessage : IMessage
    {
        public string Message { get; set; }
    }
}
