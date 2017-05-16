using System;
using System.Collections.Generic;
using System.Text;

namespace PubSubSDK
{
    public class PubSubMessage : IMessage
    {
        public string Message { get; set; }
    }
}
