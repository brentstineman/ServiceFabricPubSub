using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Data;
using System.Fabric;
using Microsoft.AspNetCore.Hosting;

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

        // POST api/subscriber/{subscriber}
        [HttpPost]
        [Route("subscriber/{subscriberId}")]
        public async Task RegisterSubscriber(string subscriberId)
        {
            var service = new TopicService(context);
            await service.RegisterSubscriber(subscriberId);
        }

        // POST api/{message}
        [HttpPost]
        public async Task Push([FromBody]string message)
        {
            var service = new TopicService(context);
            await service.Push(new PubSubDotnetSDK.PubSubMessage(message));
        }
    }
}
