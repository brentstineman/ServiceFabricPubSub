using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Fabric.Description;
using Microsoft.ServiceFabric.Data;
using Microsoft.AspNetCore.Hosting;
using System.Fabric;
using System.Fabric.Query;
using System.Text;

namespace Admin.Controllers
{
    [Route("api/")]
    public class SubscribersController : Controller
    {
        private readonly IReliableStateManager stateManager;
        private readonly FabricClient fabric;
        private readonly StatefulServiceContext serviceContext;
        private readonly string applicationName;

        public SubscribersController(IReliableStateManager stateManager, 
                            StatefulServiceContext context, 
                            FabricClient fabric)
        {
            this.stateManager = stateManager;
            this.serviceContext = context;
            this.fabric = fabric;

            applicationName = this.serviceContext.CodePackageActivationContext.ApplicationName;
        }


        // GET api/{topic}/subscribers
        [HttpGet]
        [HttpGet("{topic}/subscribers")]
        public async Task<IActionResult> Get(string topic)
        {
            //TODO filter by topic.
            ServiceList services = await this.fabric.QueryManager.GetServiceListAsync(new Uri(applicationName));

            return this.Ok(services
                            .Where(x => x.ServiceTypeName == Constants.SUBSCRIBER_SERVICE_TYPE_NAME)
                            .Select(x => new
                                    {
                                        ServiceName = x.ServiceName.ToString(),
                                        ServiceStatus = x.ServiceStatus.ToString()
                                    }));
        }


        // PUT api/{topicname}/subscribers/{name}
        [HttpPut("{topicName}/subscribers/{name}")]
        public async Task<IActionResult> Put(string topicName, string name)
        {
            ServiceList services = await this.fabric.QueryManager.GetServiceListAsync(new Uri(applicationName));

            try
            {
                Service topicService =  services.Where(x => x.ServiceTypeName == Constants.TOPIC_SERVICE_TYPE_NAME
                               && x.ServiceName.ToString().Contains($"/topics/{topicName}")).Single();
            }
            catch (InvalidOperationException ex)
            {
                //Topic is not availible
                return NotFound($"No topic found with name '{topicName}'.  Please create topic before adding subscription.");
            }

            StatefulServiceDescription serviceDescription = new StatefulServiceDescription()
            {
                ApplicationName = new Uri(this.applicationName),
                MinReplicaSetSize = 3,
                TargetReplicaSetSize = 3,
                PartitionSchemeDescription = new SingletonPartitionSchemeDescription(),
                HasPersistedState = true,
                InitializationData = Encoding.UTF8.GetBytes(topicName),
                ServiceTypeName = Constants.SUBSCRIBER_SERVICE_TYPE_NAME,
                ServiceName = CreateSubscriptionUri(topicName, name)
            };

            await fabric.ServiceManager.CreateServiceAsync(serviceDescription);

            return Ok();
        }

        private Uri CreateSubscriptionUri(string topicName, string subscriptionName)
        {
            return new Uri($"{this.serviceContext.CodePackageActivationContext.ApplicationName}/topics/{topicName}/{subscriptionName}");
        }

        // DELETE api/{topicname}/subscribers/{name}
        [HttpDelete("{topicName}/subscribers/{name}")]
        public async Task<IActionResult> Delete(string topicName, string name)
        {
            Uri serviceUri = this.CreateSubscriptionUri(topicName, name);

            var description = new DeleteServiceDescription(serviceUri);

            await fabric.ServiceManager.DeleteServiceAsync(description);

            return Ok();
        }
    }
}
