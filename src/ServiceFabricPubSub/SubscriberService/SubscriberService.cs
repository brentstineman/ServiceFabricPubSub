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
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Client;

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
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken).ConfigureAwait(false); // DELAY HACK TO AVOID STRANGER ERROR IF TOO QUICK STARTUP
            var topicSvc = ServiceProxy.Create<ITopicService>(new Uri("fabric:/PubSubTransactionPoC/Topic1"),
                 new ServicePartitionKey(0));
            await topicSvc.RegisterSubscriber(this.Context.ServiceName.Segments[2]).ConfigureAwait(false);

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task<PubSubMessage> Pop()
        {
            // TODO : replace appname & service name by value comming from configuration
            var topicSvc = ServiceProxy.Create<ITopicService>(new Uri("fabric:/PubSubTransactionPoC/Topic1"),
                new ServicePartitionKey(0));
           
            var msg = await topicSvc.InternalPop(this.Context.ServiceName.Segments[2]).ConfigureAwait(false);
            ServiceEventSource.Current.ServiceMessage(this.Context, $"NEW SUBSCRIBER MESSAGE  POP : {msg}");
            return msg;
        }
    }
}
