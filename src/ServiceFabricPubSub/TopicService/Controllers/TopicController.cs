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
    [Route("api")]
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

        [HttpPost]
        public async Task Post([FromBody]object message)
        {
            var msg = new PubSubMessage(message.ToString());
            var uri = CreateTopicUri();
            var serviceRPC = ServiceProxy.Create<ITopicService>(uri);
            await serviceRPC.Push(msg);
        }

        private Uri CreateTopicUri()
        {
            return new Uri($"{this.context.ServiceName}");
        }
    }
}
