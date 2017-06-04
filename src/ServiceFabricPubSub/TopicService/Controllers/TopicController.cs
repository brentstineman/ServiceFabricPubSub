using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Data;
using System.Fabric;
using Microsoft.AspNetCore.Hosting;
using PubSubDotnetSDK;

namespace TopicService.Controllers
{
    [Route("api")]
    public class TopicController : Controller
    {
        private readonly IReliableStateManager stateManager;
        private readonly IApplicationLifetime appLifetime;
        private readonly StatefulServiceContext context;
        private readonly ITopicService topicService;

        public TopicController(IReliableStateManager stateManager, StatefulServiceContext context, IApplicationLifetime appLifetime, ITopicService topicService)
        {
            this.stateManager = stateManager;
            this.context = context;
            this.appLifetime = appLifetime;
            this.topicService = topicService;
        }

        [HttpPost]
        public async Task Post([FromBody]object message)
        {
            await topicService.Push(new PubSubMessage(message.ToString()));
        }
    }
}
