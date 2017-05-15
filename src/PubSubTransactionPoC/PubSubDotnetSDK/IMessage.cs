using System;
using System.Collections.Generic;
using System.Text;

namespace PubSubDotnetSDK
{
    public interface IMessage
    {
        string Message { get; set; }
    }
}
