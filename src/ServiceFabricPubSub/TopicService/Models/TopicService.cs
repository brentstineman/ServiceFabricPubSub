using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using PubSubDotnetSDK;
using System;
using System.Fabric;
using System.Threading.Tasks;

namespace TopicService.Models
{
    public class TopicProvider : ITopicService
    {
        IReliableStateManager stateManager;
        StatefulServiceContext context;

        public TopicProvider(IReliableStateManager stateManager, StatefulServiceContext context)
        {
            this.stateManager = stateManager;
            this.context = context;
        }

        public Task<PubSubMessage> InternalDequeue(string subscriberId)
        {
            throw new NotImplementedException();
        }

        public Task<PubSubMessage> InternalPeek(string subscriberId)
        {
            throw new NotImplementedException();
        }

        public async Task Push(PubSubMessage msg)
        {
            msg.MessageID = Guid.NewGuid(); // generating a new unique ID for the incoming message.
            var lst = await this.stateManager.GetOrAddAsync<IReliableDictionary<string, bool>>("queueList").ConfigureAwait(false);
            var inputQueue = await this.stateManager.GetOrAddAsync<IReliableQueue<PubSubMessage>>("inputQueue");
            using (var tx = this.stateManager.CreateTransaction())
            {
                await inputQueue.EnqueueAsync(tx, msg).ConfigureAwait(false);
                await tx.CommitAsync().ConfigureAwait(false);
            }
            ServiceEventSource.Current.ServiceMessage(context, $"INPUT QUEUE: {msg.Message}");
        }

        public Task RegisterSubscriber(string subscriberId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UnregisterSubscriber(string subscriberId)
        {
            throw new NotImplementedException();
        }
    }
}
