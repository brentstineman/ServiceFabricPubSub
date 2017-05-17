using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Data;
using System.Fabric;
using Microsoft.AspNetCore.Hosting;
using PubSubDotnetSDK;
using System;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Client;

namespace TopicService.Controllers
{
    [Route("api/[controller]")]
    public class TopicController : Controller
    {
        private readonly IReliableStateManager stateManager;
        private readonly IApplicationLifetime appLifetime;
        private readonly StatefulServiceContext context;

        public TopicController(IReliableStateManager stateManager, StatefulServiceContext context, IApplicationLifetime appLifetime)
        {
            this.stateManager = stateManager;
            this.context = context;
            this.appLifetime = appLifetime;
        }

        // PUT api/topic/{topic}
        [HttpPut]
        public async Task Push(string topic, [FromBody]string message)
        {
            var service = new TopicService(context);
            var msg = new PubSubMessage(message);
            var serviceRPC = ServiceProxy.Create<ITopicService>(new Uri($"fabric:/TenantApplication/{topic}"),
               new ServicePartitionKey(0));
            await serviceRPC.Push(msg);
        }
        
        [HttpGet]
        [Route("")]
        public async Task Test(string subscriberId)
        {
            return;
        }
    }
}
