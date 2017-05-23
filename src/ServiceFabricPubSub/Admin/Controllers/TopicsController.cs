﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Fabric.Description;
using Microsoft.ServiceFabric.Data;
using System.Fabric;
using System.Fabric.Query;
using Admin.Models;

namespace Admin.Controllers
{
    [Route("api/[controller]")]
    public class TopicsController : Controller
    {
        private readonly IReliableStateManager stateManager;
        private readonly FabricClient fabric;
        private readonly StatefulServiceContext serviceContext;
        private readonly string applicationName;
        private readonly ISubscriberFabricProvider subscribeProvider;

        public TopicsController(IReliableStateManager stateManager, 
                            StatefulServiceContext context, 
                            FabricClient fabric,
                            ISubscriberFabricProvider subscribeProvider)
        {
            this.stateManager = stateManager;
            this.serviceContext = context;
            this.fabric = fabric;
            this.subscribeProvider = subscribeProvider;

            applicationName = this.serviceContext.CodePackageActivationContext.ApplicationName;
        }


        // GET api/topics
        [HttpGet]
        public async Task<IActionResult> Get()
        {

            ServiceList services = await this.fabric.QueryManager.GetServiceListAsync(new Uri(applicationName));

            return this.Ok(services
                            .Where(x => x.ServiceTypeName == Constants.TOPIC_SERVICE_TYPE_NAME)
                            .Select(x => new
                                    {
                                        ServiceName = x.ServiceName.ToString(),
                                        ServiceStatus = x.ServiceStatus.ToString()
                                    }));
        }

        // GET api/topics/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            await Task.Delay(1);
            return Ok("Coming soon");
        }

        // PUT api/topics/topicname
        [HttpPut("{name}")]
        public async Task<IActionResult> Put(string name)
        {
            StatefulServiceDescription serviceDescription = new StatefulServiceDescription()
            {
                ApplicationName = new Uri(this.applicationName),
                MinReplicaSetSize = 3,
                TargetReplicaSetSize = 3,
                PartitionSchemeDescription = new SingletonPartitionSchemeDescription(),
                HasPersistedState = true,
                ServiceTypeName = Constants.TOPIC_SERVICE_TYPE_NAME,
                ServiceName = this.serviceContext.CreateTopicUri(name)
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


        // DELETE api/topics/topicname
        [HttpDelete("{name}")]
        public async Task<IActionResult> Delete(string name)
        {
            var result = await this.subscribeProvider.GetSubscribers(name, applicationName);
            foreach(var r in result)
            {
                await DeleteService(r.ServiceName);
            }

            await DeleteService(this.serviceContext.CreateTopicUri(name));

            return Ok();
        }

        private async Task DeleteService(string serviceName)
        {
            var targetUri = new Uri(serviceName);
            await DeleteService(targetUri);
        }

        private async Task DeleteService(Uri targetUri)
        {
            var description = new DeleteServiceDescription(targetUri);
            try
            {
                await fabric.ServiceManager.DeleteServiceAsync(description);
            }
            catch (FabricElementNotFoundException)
            {
                // service doesn't exist; nothing to delete
            }
        }
    }
}
