using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;

namespace Admin
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance. 
    /// </summary>
    internal sealed class Admin : StatefulService
    {

        private const string KEY1 = "key1";
        private const string KEY2 = "key2";
        private const string COLLECTION_KEYS = "keys";

        public Admin(StatefulServiceContext context)
            : base(context)
        { }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new List<ServiceReplicaListener>()
            {
                new ServiceReplicaListener(
                    context =>
                        new KestrelCommunicationListener(
                            context,
                            (url, listener) =>
                            {
                                ServiceEventSource.Current.Message($"Listening on {url}");

                                return new WebHostBuilder()
                                    .UseKestrel()
                                    .ConfigureServices(
                                        services => services
                                            .AddSingleton<StatefulServiceContext>(this.Context)
                                            .AddSingleton<IReliableStateManager>(this.StateManager)
                                             .AddSingleton<FabricClient>(new FabricClient()))
                                    .UseContentRoot(Directory.GetCurrentDirectory())
                                    .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.UseUniqueServiceUrl)
                                    .UseStartup<Startup>()
                                    .UseUrls(url)
                                    .Build();
                            })
                    )
            };
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // Generate keys only once
            // have to check first to see if it's already been initialized
            // because RunAsync will run multiple times throughout a service's lifetime 
            await GenerateServiceKeys();
        }

        private async Task GenerateServiceKeys()
        {
            var topics = await GetTopicDictionary();

            using (var tx = this.StateManager.CreateTransaction())
            {
                var key1 = await topics.TryGetValueAsync(tx, KEY1);

                var isKey1Initialized = key1.HasValue && !string.IsNullOrWhiteSpace(key1.Value);
                if (!isKey1Initialized)
                {
                    string newKey = GenerateNewKey();
                    await topics.TryAddAsync(tx, KEY1, newKey);
                }

                var key2 = await topics.TryGetValueAsync(tx, KEY2);

                var isKey2Initialized = key2.HasValue && !string.IsNullOrWhiteSpace(key2.Value);
                if (!isKey2Initialized)
                {
                    string newKey = GenerateNewKey();
                    await topics.TryAddAsync(tx, KEY2, newKey);
                }

                await tx.CommitAsync();
            }
        }

        private string GenerateNewKey()
        {
            // TODO generator a valid security key. using basic placeholder for now
            // should also encrypt as the key is stored in plain text when stored in
            // statefule service
            return Guid.NewGuid().ToString();
        }

        private async Task<IReliableDictionary<string, string>> GetTopicDictionary()
        {
            return await this.StateManager.GetOrAddAsync<IReliableDictionary<string, string>>(COLLECTION_KEYS);
        }
    }
}