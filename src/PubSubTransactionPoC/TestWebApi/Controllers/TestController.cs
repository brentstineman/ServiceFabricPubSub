using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using PubSubDotnetSDK;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Web.Http;

namespace TestWebApi.Controllers
{
    [ServiceRequestActionFilter]
    public class TestController : ApiController
    {        
        [HttpGet]
        public async Task<PubSubMessage> Pop()
        {            
            var stockSvc = ServiceProxy.Create<ITopicService>(new Uri("fabric:/PubSubTransactionPoC/Topic1"),
               new ServicePartitionKey(0));
            return await stockSvc.InternalPop();            
        }
        
        [HttpPost]
        // POST api/values 
        public async Task Push([FromBody]string value)
        {
            var msg = new PubSubMessage() { Message = value };
            var stockSvc = ServiceProxy.Create<ITopicService>(new Uri("fabric:/PubSubTransactionPoC/Topic1"),
               new ServicePartitionKey(0));            
            await stockSvc.Push(msg);
        }
        
    }
}
