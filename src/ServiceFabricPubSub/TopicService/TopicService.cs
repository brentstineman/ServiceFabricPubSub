using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using PubSubDotnetSDK;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Data.Notifications;
using TopicService.Models;

namespace TopicService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class TopicService : StatefulService, ITopicService
    {
        public TopicService(StatefulServiceContext context)
            : base(context)
        { }

         

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new List<ServiceReplicaListener>()
            {
                new ServiceReplicaListener( (context) => this.CreateServiceRemotingListener(context),"ServiceEndpoint1" ),
                new ServiceReplicaListener(serviceContext =>
                    new KestrelCommunicationListener(
                        serviceContext,
                        (url, listener) => new WebHostBuilder().UseKestrel().ConfigureServices(
                             services => services
                                 .AddSingleton<StatefulServiceContext>(this.Context)
                                 .AddSingleton<IReliableStateManager>(this.StateManager)
                                 .AddSingleton<ITopicService>(new TopicProvider(this.StateManager, this.Context)))
                        .UseContentRoot(Directory.GetCurrentDirectory())
                        .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.UseUniqueServiceUrl)
                        .UseStartup<Startup>()
                        .UseUrls(url)
                        .Build()),"ServiceEndpoint2")
            };
        }


        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            this.StateManager.StateManagerChanged += StateManager_StateManagerChanged;
            this.StateManager.TransactionChanged += StateManager_TransactionChanged;

            //int count = 1; // HACK used for testmessage autogeneration
            //while (true)
            //{
            //    cancellationToken.ThrowIfCancellationRequested();
            //    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

            //    // HACK CREATE TEST MESSAGE
            //    // TODO remove this next 2 line when CLI available
            //    var testMsg = new PubSubMessage() { Message = $"TEST Message #{count++} : {DateTime.Now}" };
            //    await Push(testMsg); // HACK FOR TEST
            //}
        }


        private void StateManager_TransactionChanged(object sender, NotifyTransactionChangedEventArgs e)
        {
            // TODO : check if event target inputQueue only (no need to react on over state change)

            // transaction commited
            if (e.Action == NotifyTransactionChangedAction.Commit)
            {
                Task.Run(() => DuplicateMessages(CancellationToken.None));
            }
        }

        private void StateManager_StateManagerChanged(object sender, NotifyStateManagerChangedEventArgs e)
        {
            // TODO : check if event target inputQueue only (no need to react on over state change)

            // state manager created
            if (e.Action == NotifyStateManagerChangedAction.Add)
            {
                Task.Run(() => DuplicateMessages(CancellationToken.None));
            }
        }

        private async Task DuplicateMessages(CancellationToken cancellationToken)
        {
            // get input q
            var inputQueue = await this.StateManager.GetOrAddAsync<IReliableQueue<PubSubMessage>>("inputQueue");            

            var lst = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, bool>>("queueList");

            using (var tx = this.StateManager.CreateTransaction())
            {
                // enq 1 message
                while (await inputQueue.GetCountAsync(tx).ConfigureAwait(false) > 0)
                {
                    var msg = await inputQueue.TryDequeueAsync(tx).ConfigureAwait(false);
                    if (!msg.HasValue) return;
                    IAsyncEnumerable<KeyValuePair<string, bool>> asyncEnumerable = await lst.CreateEnumerableAsync(tx).ConfigureAwait(false);
                    using (IAsyncEnumerator<KeyValuePair<string, bool>> asyncEnumerator = asyncEnumerable.GetAsyncEnumerator())
                    {
                        while (await asyncEnumerator.MoveNextAsync(CancellationToken.None).ConfigureAwait(false))
                        {
                            var queue = await this.StateManager.GetOrAddAsync<IReliableQueue<PubSubMessage>>(asyncEnumerator.Current.Key);
                            await queue.EnqueueAsync(tx, msg.Value).ConfigureAwait(false);
                        }
                        ServiceEventSource.Current.ServiceMessage(this.Context, $"ENQUEUE: {msg.Value.Message} into {asyncEnumerator.Current.Key}");
                    }
                }
                await tx.CommitAsync().ConfigureAwait(false);

            }
        }

        /// <summary>
        /// Remove a subscriber ID from the known subscriber , and delete outputqueue for that subscriber.
        /// </summary>
        /// <param name="subscriberId"></param>
        /// <returns></returns>
        public async Task<bool> UnregisterSubscriber(string subscriberId)
        {
            var lst = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, bool>>("queueList");
            var queueName = $"queue_{subscriberId}";
            using (var tx = this.StateManager.CreateTransaction())
            {
                if (await lst.ContainsKeyAsync(tx, queueName).ConfigureAwait(false))
                {
                    // subscriber to remove found -> removing it from lst and deleting associated queue
                    var remResult = await lst.TryRemoveAsync(tx, queueName).ConfigureAwait(false);
                    if (remResult.HasValue && remResult.Value == false) // failure to remove entry from dictio
                    {
                        return false;
                    }

                    await this.StateManager.RemoveAsync(tx, queueName);
                    await tx.CommitAsync().ConfigureAwait(false);
                    return true;
                }
                else
                {
                    tx.Abort(); // nothing to update -> cancel the tx
                    return false;
                }
            }
        }

        /// <summary>
        /// Create a new outputqueue for a new subscriber instance
        /// </summary>
        /// <param name="subscriberId"></param>
        /// <returns></returns>
        public async Task RegisterSubscriber(string subscriberId)
        {
            var queueName = $"queue_{subscriberId}";

            var lst = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, bool>>("queueList").ConfigureAwait(false);
            using (var tx = this.StateManager.CreateTransaction())
            {
                if (!await lst.ContainsKeyAsync(tx, queueName).ConfigureAwait(false))
                {
                    await lst.AddAsync(tx, queueName, true).ConfigureAwait(false);
                }
                await tx.CommitAsync().ConfigureAwait(false);
            }

        }


     
        /// <summary>
        /// Enqueue a new message in the topic
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public async Task Push(PubSubMessage msg)
        {
            msg.MessageID = Guid.NewGuid(); // generating a new unique ID for the incoming message.
            var lst = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, bool>>("queueList").ConfigureAwait(false);
            var inputQueue = await this.StateManager.GetOrAddAsync<IReliableQueue<PubSubMessage>>("inputQueue");
            using (var tx = this.StateManager.CreateTransaction())
            {
                await inputQueue.EnqueueAsync(tx, msg).ConfigureAwait(false);
                await tx.CommitAsync().ConfigureAwait(false);
            }
            ServiceEventSource.Current.ServiceMessage(this.Context, $"INPUT QUEUE: {msg.Message}");
            
        }


        /// <summary>
        /// Peek message from the queue (to be used by subscriber for pseudo transactionnal behavior)
        /// </summary>
        /// <param name="subcriberId"></param>
        /// <returns></returns>
        public async Task<PubSubMessage> InternalPeek(string subscriberId)
        {
            return await RunOnOutputQueue(subscriberId, (q, tx) => q.TryPeekAsync(tx)).ConfigureAwait(false);
        }

        /// <summary>
        /// Dequeue message from the queue (to be used by subscriber for pseudo transactionnal behavior)
        /// </summary>
        /// <param name="subcriberId"></param>
        /// <returns></returns>
        public async Task<PubSubMessage> InternalDequeue(string subscriberId)
        {
            return await RunOnOutputQueue(subscriberId, (q, tx) => q.TryDequeueAsync(tx)).ConfigureAwait(false);
        }

        async Task<PubSubMessage> RunOnOutputQueue(string subscriberId,
           Func<IReliableQueue<PubSubMessage>, ITransaction, Task<ConditionalValue<PubSubMessage>>> callOnQueue)
        {
            var queueName = $"queue_{subscriberId}";
            var queue = await this.StateManager.GetOrAddAsync<IReliableQueue<PubSubMessage>>(queueName).ConfigureAwait(false);
            var lst = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, bool>>("queueList").ConfigureAwait(false);

            PubSubMessage msg = null;
            using (var tx = this.StateManager.CreateTransaction())
            {
                if (!await lst.ContainsKeyAsync(tx, queueName).ConfigureAwait(false))
                {
                    await lst.AddAsync(tx, queueName, true).ConfigureAwait(false);
                }

                var msgCV = await callOnQueue(queue, tx).ConfigureAwait(false);
                //var msgCV = await q.TryDequeueAsync(tx).ConfigureAwait(false);
                if (msgCV.HasValue)
                    msg = msgCV.Value;
                await tx.CommitAsync().ConfigureAwait(false);
            }
            ServiceEventSource.Current.ServiceMessage(this.Context, $"DEQUEUE FOR {subscriberId} : {msg?.Message}");
            return msg;
        }

      
    }
}