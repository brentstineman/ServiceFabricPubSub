﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Fabric.Description;
using Microsoft.ServiceFabric.Data;
using System.Fabric;
using System.Fabric.Query;
using System.Text;
using Admin.Models;

namespace Admin.Controllers
{
    [Route("api/")]
    public class SubscribersController : Controller
    {
        private readonly IReliableStateManager stateManager;
        private readonly FabricClient fabric;
        private readonly StatefulServiceContext serviceContext;
        private readonly string applicationName;
        private readonly ISubscriberFabricProvider provider;

        public SubscribersController(
                            IReliableStateManager stateManager,
                            StatefulServiceContext context,
                            FabricClient fabric,
                            ISubscriberFabricProvider provider)
        {
            this.stateManager = stateManager;
            this.serviceContext = context;
            this.fabric = fabric;
            this.provider = provider;

            applicationName = this.serviceContext.CodePackageActivationContext.ApplicationName;
        }


        // GET api/{topic}/subscribers
        [HttpGet]
        [HttpGet("{topic}/subscribers")]
        public async Task<IActionResult> Get(string topic)
        {
            //TODO filter by topic.
            return this.Ok(await provider.GetSubscribers(topic, applicationName));
        }


        // PUT api/{topicname}/subscribers/{name}
        [HttpPut("{topicName}/subscribers/{name}")]
        public async Task<IActionResult> Put(string topicName, string name)
        {
            ServiceList services = await this.fabric.QueryManager.GetServiceListAsync(new Uri(applicationName));

            try
            {
                Service topicService = services.Where(x => x.ServiceTypeName == Constants.TOPIC_SERVICE_TYPE_NAME
                              && x.ServiceName.IsTopic(topicName)).Single();
            }
            catch (InvalidOperationException)
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

            try
            {
                await fabric.ServiceManager.CreateServiceAsync(serviceDescription);
            }
            catch (FabricElementAlreadyExistsException)
            {
                //idempotent so return 200
                return Ok();
            }

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
            try
            {
                await fabric.ServiceManager.DeleteServiceAsync(description);

            }
            catch (FabricElementNotFoundException)
            {
                // service doesn't exist; nothing to delete
                return Ok();
            }

            return Ok();
        }
    }
}
