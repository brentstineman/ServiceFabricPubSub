using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Data;
using System.Fabric;
using Microsoft.AspNetCore.Hosting;
using PubSubDotnetSDK;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System;
using System.Threading.Tasks;

namespace SubscriberService.Controllers
{
    [Route("api")]
    public class SubscriberController : Controller
    {
        private readonly IReliableStateManager stateManager;
        private readonly IApplicationLifetime appLifetime;
        private readonly StatefulServiceContext context;

        public SubscriberController(IReliableStateManager stateManager, StatefulServiceContext context, IApplicationLifetime appLifetime)
        {
            this.stateManager = stateManager;
            this.context = context;
            this.appLifetime = appLifetime;
        }

        [HttpGet]
        public async Task<PubSubMessage> Pop()
        {
            var uri = CreateSubscriberUri();
            var serviceRPC = ServiceProxy.Create<ISubscriberService>(uri);
            return await serviceRPC.Pop();
        }

        private Uri CreateSubscriberUri()
        {
            return new Uri($"{this.context.ServiceName}");
        }
    }
}
