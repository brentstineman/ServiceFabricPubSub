using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using PubSubDotnetSDK;
using System.Threading.Tasks;

namespace SubscriberService.Models
{
    public class SubscriberProvider : ISubscriberService
    {
        IReliableStateManager stateManager;

        public SubscriberProvider(IReliableStateManager stateManager)
        {
            this.stateManager = stateManager;
        }

        public async Task<long> CountAsync()
        {
            // TODO OPTIMISATION :  try to optimze count method by maintaining an internal counter to avoid transactionnal call on queue, and increasing scalability
            //        internal counter value will be initialised in service Startup , and update each time of queue/dequeue transaction occurs.
            var queue = await this.stateManager.GetOrAddAsync<IReliableQueue<PubSubMessage>>("messages").ConfigureAwait(false);
            long count = 0;
            using (var tx = this.stateManager.CreateTransaction()) // ReliableQueue need TX for count().
            {
                count = await queue.GetCountAsync(tx).ConfigureAwait(false);
                tx.Abort();
            }
            return count;
        }

        public async Task<PubSubMessage> Pop()
        {
            PubSubMessage msg = null;
            var queue = await this.stateManager.GetOrAddAsync<IReliableQueue<PubSubMessage>>("messages").ConfigureAwait(false);
            using (var tx = this.stateManager.CreateTransaction())
            {
                var msgCV = await queue.TryDequeueAsync(tx).ConfigureAwait(false);
                if (msgCV.HasValue)
                    msg = msgCV.Value;
                await tx.CommitAsync().ConfigureAwait(false);
            }
            return msg;
        }
    }
}
