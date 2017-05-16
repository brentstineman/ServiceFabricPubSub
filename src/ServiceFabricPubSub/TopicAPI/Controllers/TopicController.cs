using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Data;
using System.Fabric;
using Microsoft.AspNetCore.Hosting;

namespace TopicAPI.Controllers
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

        // GET api/topic/{topic} 
        [HttpGet]
        [Route("{topic}")]
        public async Task<object> Get(string topic)
        {
            return await new QueueManagerProvider(this.stateManager).GetAsync(topic);
        }

        // PUT api/topic/{topic} 
        [HttpPut]
        [Route("{topic}")]
        public async Task Put(string topic, [FromBody]object message)
        {
            await new QueueManagerProvider(this.stateManager).PutAsync(message, topic);
            return;
        }
    }
}
