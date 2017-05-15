using System;
using System.Collections.Generic;
using System.Text;

namespace PubSubDotnetSDK
{
    public class PubSubMessage : IMessage
    {
        public PubSubMessage()
        {
            this.Message = null;
        }

        public string Message { get; set; }
    }
}
