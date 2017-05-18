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
using System.Fabric.Query;
using System.Fabric.Description;
using PubSubDotnetSDK;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace Admin
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance. 
    /// </summary>
    internal sealed class Admin : Microsoft.ServiceFabric.Services.Runtime.StatefulService
    {
        private readonly TimeSpan OffloadBatchInterval = TimeSpan.FromSeconds(2);

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


        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // Generate keys only once
            // have to check first to see if it's already been initialized
            // because RunAsync will run multiple times throughout a service's lifetime 
            await GenerateServiceKeys();


            // check for subscribers that have been registered for deletion.
            var subscriptions = await this.StateManager.GetRemoveSubscriptionsDictionary();
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    await FinishRemovalOfSubscriptions(subscriptions);
                }
                catch (TimeoutException)
                {
                    // transient error. Retry.
                    ServiceEventSource.Current.ServiceMessage(this.Context, "TimeoutException in RunAsync.");
                }
                catch (FabricTransientException fte)
                {
                    // transient error. Retry.
                    ServiceEventSource.Current.ServiceMessage(this.Context, "FabricTransientException in RunAsync: {0}", fte.Message);
                }
                catch (FabricNotPrimaryException)
                {
                    // not primary any more, time to quit.
                    return;
                }

                await Task.Delay(this.OffloadBatchInterval, cancellationToken);
            }
        }

        private async Task FinishRemovalOfSubscriptions(IReliableDictionary<string, string> subscriptions)
        {
            using (ITransaction tx = this.StateManager.CreateTransaction())
            {
                var unregisteredServices = new List<string>();

                IAsyncEnumerable<KeyValuePair<string, string>> asyncEnumerable = await subscriptions.CreateEnumerableAsync(tx).ConfigureAwait(false);
                using (IAsyncEnumerator<KeyValuePair<string, string>> asyncEnumerator = asyncEnumerable.GetAsyncEnumerator())
                {
                    while (await asyncEnumerator.MoveNextAsync(CancellationToken.None).ConfigureAwait(false))
                    {
                        string subscriber = asyncEnumerator.Current.Key;
                        string topic = asyncEnumerator.Current.Value;

                        try
                        {
                            // https://social.msdn.microsoft.com/Forums/en-US/ce8aff1d-6246-4b53-9075-13b738a24b13/best-way-to-determine-if-a-service-already-exists?forum=AzureServiceFabric
                            // treat as "Desired state management"
                            // "Instead treat this more like desired state management - create the service until you are told it already exists."
                            Uri serviceUri = this.Context.CreateSubscriptionUri(topic, subscriber);
                            var description = new DeleteServiceDescription(serviceUri);

                            using (var fabric = new FabricClient())
                            {
                                await fabric.ServiceManager.DeleteServiceAsync(description);
                            }
                        }
                        catch (FabricElementNotFoundException)
                        {
                            await UnregisterSubscriberFromTopic(subscriber, topic);
                            unregisteredServices.Add(subscriber);
                        }
                    }
                }

                // do after enumerator is complete.
                foreach (var sub in unregisteredServices)
                {
                    await subscriptions.TryRemoveAsync(tx, sub);
                }

                await tx.CommitAsync();
            }

        }

        private async Task UnregisterSubscriberFromTopic(string subscriber, string topic)
        {
            ServiceEventSource.Current.ServiceMessage(this.Context, $"Service has been removed.  Unregistering from topic '{topic}'... ");

            var uri = this.Context.CreateTopicUri(topic);
            var serviceRPC = ServiceProxy.Create<ITopicService>(uri);

            try
            {
                await serviceRPC.UnregisterSubscriber(subscriber);
            }
            catch (FabricServiceNotFoundException)
            {
                // topic servic
                ServiceEventSource.Current.ServiceMessage(this.Context, $"Topic Service no longer exists for '{topic}'.");
            }
            
            ServiceEventSource.Current.ServiceMessage(this.Context, $"Unregistered from topic '{topic}'.");
        }

        private async Task GenerateServiceKeys()
        {
            var topics = await GetKeysDictionary();

            using (var tx = this.StateManager.CreateTransaction())
            {
                var key1 = await topics.TryGetValueAsync(tx, Constants.KEY1);

                var isKey1Initialized = key1.HasValue && !string.IsNullOrWhiteSpace(key1.Value);
                if (!isKey1Initialized)
                {
                    string newKey = GenerateNewKey();
                    await topics.TryAddAsync(tx, Constants.KEY1, newKey);
                }

                var key2 = await topics.TryGetValueAsync(tx, Constants.KEY2);

                var isKey2Initialized = key2.HasValue && !string.IsNullOrWhiteSpace(key2.Value);
                if (!isKey2Initialized)
                {
                    string newKey = GenerateNewKey();
                    await topics.TryAddAsync(tx, Constants.KEY2, newKey);
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

        private async Task<IReliableDictionary<string, string>> GetKeysDictionary()
        {
            return await this.StateManager.GetOrAddAsync<IReliableDictionary<string, string>>(Constants.COLLECTION_KEYS);
        }
    }
}