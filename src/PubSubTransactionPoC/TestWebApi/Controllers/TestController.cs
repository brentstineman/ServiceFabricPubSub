using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using PubSubDotnetSDK;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Web.Http;
using System.Fabric;

namespace TestWebApi.Controllers
{
    [ServiceRequestActionFilter]
    public class TestController : ApiController
    {        
        //[HttpGet]       
        //public async Task<PubSubMessage> InternalPop()
        //{            
        //    var stockSvc = ServiceProxy.Create<ITopicService>(new Uri("fabric:/PubSubTransactionPoC/Topic1"),
        //       new ServicePartitionKey(0));
        //    return await stockSvc.InternalPop();            
        //}

        [HttpGet]
        public async Task<PubSubMessage> Pop(string subscriberName)
        {
            string name = subscriberName ??
                FabricRuntime.GetActivationContext()
                    .GetConfigurationPackageObject("Config")
                    .Settings
                    .Sections["WebApiConfigSection"]
                    .Parameters["SubscriberName"]
                    .Value;
            var svcName = $"fabric:/PubSubTransactionPoC/{name}";
            var stockSvc = ServiceProxy.Create<ISubscriberService>(new Uri(svcName),new ServicePartitionKey(0));
            
            var msg = await stockSvc.Pop();

            return msg;
        }

        [HttpPost]
        // POST api/values 
        public async Task Push([FromBody]string value)
        {
            string name =
             FabricRuntime.GetActivationContext()
                 .GetConfigurationPackageObject("Config")
                 .Settings
                 .Sections["WebApiConfigSection"]
                 .Parameters["TopicName"]
                 .Value;

            var msg = new PubSubMessage() { Message = value };
            var stockSvc = ServiceProxy.Create<ITopicService>(new Uri("fabric:/PubSubTransactionPoC/" + name),
               new ServicePartitionKey(0));            
            await stockSvc.Push(msg);
        }
        
    }
}
