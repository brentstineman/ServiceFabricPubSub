using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PubSubDotnetSDK
{
    public interface ISubscriberService
    {
        Task<IMessage> Pop();
    }
}
