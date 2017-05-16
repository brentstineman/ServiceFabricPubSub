using System;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using System.Threading.Tasks;

namespace TopicAPI
{
    class QueueManagerProvider
    {
        IReliableStateManager _StateManager { get; set; }

        public QueueManagerProvider(IReliableStateManager stateManager)
        {
            _StateManager = stateManager;
        }

        public async Task PutAsync(object message, string topic)
        {
            try
            {
                var topicQueue = await _StateManager.GetOrAddAsync<IReliableQueue<object>>(topic);

                // Create a new Transaction object for this partition
                using (ITransaction tx = _StateManager.CreateTransaction())
                {
                    // AddAsync takes key's write lock; if >4 secs, TimeoutException
                    // Key & value put in temp dictionary (read your own writes),
                    // serialized, redo/undo record is logged & sent to
                    // secondary replicas
                    await topicQueue.EnqueueAsync(tx, message.ToString());

                    // CommitAsync sends Commit record to log & secondary replicas
                    // After quorum responds, all locks released
                    await tx.CommitAsync();
                }
                // If CommitAsync not called, Dispose sends Abort
                // record to log & all locks released
            }
            catch (Exception e)
            {
                throw e;
                //await Task.Delay(100, cancellationToken); goto retry;
            }
        }

        public async Task<object> GetAsync(string topic)
        {
            try
            {
                var topicQueue = await _StateManager.GetOrAddAsync<IReliableQueue<object>>(topic);

                // Create a new Transaction object for this partition
                using (ITransaction tx = _StateManager.CreateTransaction())
                {
                    // AddAsync takes key's write lock; if >4 secs, TimeoutException
                    // Key & value put in temp dictionary (read your own writes),
                    // serialized, redo/undo record is logged & sent to
                    // secondary replicas
                    var result = await topicQueue.TryDequeueAsync(tx);

                    // CommitAsync sends Commit record to log & secondary replicas
                    // After quorum responds, all locks released
                    await tx.CommitAsync();

                    return result.Value;
                }
                // If CommitAsync not called, Dispose sends Abort
                // record to log & all locks released
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
