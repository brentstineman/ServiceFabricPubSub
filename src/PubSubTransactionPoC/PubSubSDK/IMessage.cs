using System;
using System.Collections.Generic;
using System.Text;

namespace PubSubSDK
{
    interface IMessage
    {
        string Message { get; set; }
    }
}
