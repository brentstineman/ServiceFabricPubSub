using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using PubSubDotnetSDK;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Client;
using System.Text;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.ServiceFabric.Data;
using Microsoft.Extensions.DependencyInjection;

namespace SubscriberService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class SubscriberService : StatefulService, ISubscriberService
    {
        public SubscriberService(StatefulServiceContext context)
            : base(context)
        { }

        
        // TODO OPTIMISATION : migrate from ReliableQueue to ReliableConcurrentQueue for higher scalability (if strict delivering order not needed Or implement a ConcurrentSubcriberService for handling both case). 


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
                new ServiceReplicaListener( (context) => this.CreateServiceRemotingListener(context), "ServiceEndpoint1"),
                new ServiceReplicaListener(serviceContext =>
                    new KestrelCommunicationListener(
                        serviceContext,
                        (url, listener) => new WebHostBuilder().UseKestrel().ConfigureServices(
                             services => services
                                 .AddSingleton<StatefulServiceContext>(this.Context)
                                 .AddSingleton<IReliableStateManager>(this.StateManager))
                        .UseContentRoot(Directory.GetCurrentDirectory())
                        .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.UseUniqueServiceUrl)
                        .UseStartup<Startup>()
                        .UseUrls(url)
                        .Build()),"ServiceEndpoint2")
            };
        }

        private Uri CreateTopicUri(string topicName)
        {
            var url = $"{this.Context.CodePackageActivationContext.ApplicationName}/topics/{topicName}";
            return new Uri(url);
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // topicname sent while creating service this way: StatefulServiceDescription.InitializationData = Encoding.UTF8.GetBytes(parameters),
            string topicName = Encoding.UTF8.GetString(this.Context.InitializationData);

            topicName = string.IsNullOrEmpty(topicName) ? this.Context
                .CodePackageActivationContext
                .GetConfigurationPackageObject("Config")
                 .Settings
                 .Sections["SubscriberConfiguration"]
                 .Parameters["TopicServiceName"]
                 .Value : topicName;

            // TODO : check the topic name value (case where no initialization & no settings --> report error)

            Uri topicUri = CreateTopicUri(topicName);
            var topicSvc = ServiceProxy.Create<ITopicService>(topicUri, new ServicePartitionKey());

            // register the subscriber to the targeted topic service in order to receive copy of message
            await topicSvc.RegisterSubscriber(this.Context.ServiceName.Segments[2]).ConfigureAwait(false);


            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken).ConfigureAwait(false);
                try
                {
                    // TODO : TEST & catch error on calling topic : if exception, recreate the serviceproxy. May occured if topic hosting node changes
                    var serviceName = this.Context.ServiceName.Segments[2];

                    // EXPLANATION : a pseudo 'transactionnal' behavior is implemented using a 2 step message peek&Delete from the Topic.
                    //     message is peeked in topic, inserted in the local queue, then (if no exception) deleted in the topic
                    //     PROS : Easy to implement, CONS : no peek batch possible, impossible to have multiple subscriber on the same output queue

                    // local queue used to persist message in the subscriber.
                    var queue = await this.StateManager.GetOrAddAsync<IReliableQueue<PubSubMessage>>("messages").ConfigureAwait(false);
                    var syncStatusDico = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, Guid>>("syncTracking").ConfigureAwait(false);

                    // implementation of message pump from TopicSvc to local queue.
                    var peekedMsg = await topicSvc.InternalPeek(serviceName).ConfigureAwait(false); // peek 1st message from topic
                    while (peekedMsg != null)
                    {
                        using (var tx = this.StateManager.CreateTransaction())
                        {
                            //retriev the last messageid inserted in the queue
                            var lastMsgId = await syncStatusDico.TryGetValueAsync(tx, "lastMessageId");

                            // if no value (first msg) or last msgid != peeked msg id -> add to queue

                            if ((!lastMsgId.HasValue) || (lastMsgId.HasValue && lastMsgId.Value!=peekedMsg.MessageID ))

                            {
                                // add the message to the local queue  
                                await queue.EnqueueAsync(tx, peekedMsg);

                                // update the lastMessageId enqueued
                                await syncStatusDico.AddOrUpdateAsync(tx, "lastMessageId", peekedMsg.MessageID, (k, id) => peekedMsg.MessageID);

                                await tx.CommitAsync().ConfigureAwait(false);
                                ServiceEventSource.Current.ServiceMessage(this.Context, $"Subscriber:{serviceName}:LocalEnqueue : msg : {peekedMsg.Message}");
                            }
                        }

                        // confirm the local enqueuing to the topicservice by dequeuing the last peeked message
                        var dequeuedMsg = await topicSvc.InternalDequeue(this.Context.ServiceName.Segments[2]).ConfigureAwait(false);
                        // checking 

                        // try peek next message from topic, if null lopp will terminate
                        peekedMsg = await topicSvc.InternalPeek(this.Context.ServiceName.Segments[2]).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    ServiceEventSource.Current.ServiceMessage(this.Context, $"EXCEPTION: {ex.ToString()}");
                }
            }
        }

        /// <summary>
        /// Implementation of ISubscriberService.Pop() method
        /// </summary>
        /// <returns>count dequeue PubSubMessage or null if queue empty</returns>
        public async Task<PubSubMessage> Pop()
        {
            PubSubMessage msg = null;
            var queue = await this.StateManager.GetOrAddAsync<IReliableQueue<PubSubMessage>>("messages").ConfigureAwait(false);
            using (var tx = this.StateManager.CreateTransaction())
            {
                var msgCV = await queue.TryDequeueAsync(tx).ConfigureAwait(false);
                if (msgCV.HasValue)
                    msg = msgCV.Value;
                await tx.CommitAsync().ConfigureAwait(false);
            }
            return msg;
        }

        /// <summary>
        /// Implementation of ISubscriberService.CountAsync() method
        /// </summary>
        /// <returns>count of messages in the queue</returns>
        public async Task<long> CountAsync()
        {
            // TODO OPTIMISATION :  try to optimze count method by maintaining an internal counter to avoid transactionnal call on queue, and increasing scalability
            //        internal counter value will be initialised in service Startup , and update each time of queue/dequeue transaction occurs.
            var queue = await this.StateManager.GetOrAddAsync<IReliableQueue<PubSubMessage>>("messages").ConfigureAwait(false);
            long count = 0;
            using (var tx = this.StateManager.CreateTransaction()) // ReliableQueue need TX for count().
            {
                count = await queue.GetCountAsync(tx).ConfigureAwait(false);
                tx.Abort();
            }
            return count;
        }
    }
}
