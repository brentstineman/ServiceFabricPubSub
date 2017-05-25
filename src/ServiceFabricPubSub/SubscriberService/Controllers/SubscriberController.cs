using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Data;
using System.Fabric;
using Microsoft.AspNetCore.Hosting;
using PubSubDotnetSDK;
using System.Threading.Tasks;

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

        [HttpGet]
        public async Task<PubSubMessage> Pop()
        {
            return await subscriberService.Pop();
        }

        [HttpGet("count")]
        public async Task<long> Count()
        {
            return await subscriberService.CountAsync();
        }
    }
}
