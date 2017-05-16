using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Fabric.Description;
using Microsoft.ServiceFabric.Data;
using Microsoft.AspNetCore.Hosting;
using System.Fabric;

namespace Admin.Controllers
{
    [Route("api/[controller]")]
    public class TopicsController : Controller
    {
        private const string TOPIC_SERVICE_NAME = "TopicServiceType";

        private readonly IReliableStateManager stateManager;
        private readonly FabricClient fabric;
        private readonly StatefulServiceContext serviceContext;

        public TopicsController(IReliableStateManager stateManager, 
                            StatefulServiceContext context, 
                            FabricClient fabric)
        {
            this.stateManager = stateManager;
            this.serviceContext = context;
            this.fabric = fabric;
        }


        // GET api/topics
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok("Coming Soon");
        }

        // GET api/topics/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            return Ok("Coming soon");
        }

        // PUT api/topics/topicname
        [HttpPut("{name}")]
        public async Task<IActionResult> Put(string name)
        {
            string applicationName = this.serviceContext.CodePackageActivationContext.ApplicationName;

            StatefulServiceDescription serviceDescription = new StatefulServiceDescription()
            {
                ApplicationName = new Uri(applicationName),
                MinReplicaSetSize = 3,
                TargetReplicaSetSize = 3,
                PartitionSchemeDescription = new UniformInt64RangePartitionSchemeDescription()
                {
                    LowKey = 0,
                    HighKey = 10,
                    PartitionCount = 1
                },
                HasPersistedState = true,
                ServiceTypeName = TOPIC_SERVICE_NAME,
                ServiceName = CreateTopicUri(name)
            };

            await fabric.ServiceManager.CreateServiceAsync(serviceDescription);

            return Ok();
        }

        private Uri CreateTopicUri(string topicName)
        {
            return new Uri($"{this.serviceContext.CodePackageActivationContext.ApplicationName}/topics/{topicName}");
        }

        // DELETE api/topics/topicname
        [HttpDelete("{name}")]
        public async Task<IActionResult> Delete(string name)
        {
            Uri serviceUri = this.CreateTopicUri(name);

            var description = new DeleteServiceDescription(serviceUri);

            await fabric.ServiceManager.DeleteServiceAsync(description);

            return Ok();
        }
    }
}
