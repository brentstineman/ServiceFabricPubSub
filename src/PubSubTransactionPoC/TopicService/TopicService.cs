using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using PubSubDotnetSDK;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;

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
                new ServiceReplicaListener( (context) => this.CreateServiceRemotingListener(context) )
            };
        }

        
        

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            var mainQueue = await this.StateManager.GetOrAddAsync<IReliableQueue<PubSubMessage>>("mainQueue");

            int count = 1;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

                await Push(new PubSubMessage() { Message = $"TEST  Message #{count++} : {DateTime.Now}" }); // HACK FOR TEST
            }
        }

        /// <summary>
        /// Enqueue a new message in the topic
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public async Task Push(PubSubMessage msg)
        {
            //var mainQueue = await this.StateManager.GetOrAddAsync<IReliableQueue<PubSubMessage>>("mainQueue");
            var q1= await this.StateManager.GetOrAddAsync<IReliableQueue<PubSubMessage>>($"queue_Subscriber1");
            var q2 = await this.StateManager.GetOrAddAsync<IReliableQueue<PubSubMessage>>($"queue_Subscriber2");
            var q3 = await this.StateManager.GetOrAddAsync<IReliableQueue<PubSubMessage>>($"queue_Subscriber3");

            using (var tx = this.StateManager.CreateTransaction())
            {
                //await mainQueue.EnqueueAsync(tx, (PubSubMessage)msg);
                await q1.EnqueueAsync(tx, msg);
                await q2.EnqueueAsync(tx, msg);
                await q3.EnqueueAsync(tx, msg);
                await tx.CommitAsync();
                ServiceEventSource.Current.ServiceMessage(this.Context, $"ENQUEUE: {msg.Message}");
            }
        }

        /// <summary>
        /// HACK Method for sprint0. 
        /// Should be removed in next sprint.
        /// </summary>
        /// <param name="subcriberId"></param>
        /// <returns></returns>
        public async Task<PubSubMessage> InternalPop(string subscriberId)
        {
            var q = await this.StateManager.GetOrAddAsync<IReliableQueue<PubSubMessage>>($"queue_{subscriberId}");

            PubSubMessage msg = null;
            using (var tx = this.StateManager.CreateTransaction())
            {
                var msgCV= await q.TryDequeueAsync(tx);
                if (msgCV.HasValue)
                    msg = msgCV.Value;
                await tx.CommitAsync();
            }
            ServiceEventSource.Current.ServiceMessage(this.Context, $"DEQUEUE FOR {subscriberId} : {msg?.Message}");
            return msg;
        }
    }
}
