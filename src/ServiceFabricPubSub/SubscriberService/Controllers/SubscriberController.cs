using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Data;
using System.Fabric;
using Microsoft.AspNetCore.Hosting;
using PubSubDotnetSDK;
using System.Threading.Tasks;
using System.Net;

namespace SubscriberService.Controllers
{
    [Route("api")]
    public class SubscriberController : Controller
    {
        private readonly IReliableStateManager stateManager;
        private readonly IApplicationLifetime appLifetime;
        private readonly StatefulServiceContext context;
        private readonly ISubscriberService subscriberService;

        public SubscriberController(IReliableStateManager stateManager, StatefulServiceContext context, IApplicationLifetime appLifetime, ISubscriberService subscriberService)
        {
            this.stateManager = stateManager;
            this.context = context;
            this.appLifetime = appLifetime;
            this.subscriberService = subscriberService;
        }

      
        //for remoting
        public async Task<PubSubMessage> Pop()
        {
            return await subscriberService.Pop();
        }

        [HttpGet]
        public PubSubMessage GetPop()
        {
            var msg = subscriberService.Pop().Result;
            return msg;
        }

        [HttpGet("count")]
        public async Task<long> Count()
        {
            return await subscriberService.CountAsync();
        }
    }
}
