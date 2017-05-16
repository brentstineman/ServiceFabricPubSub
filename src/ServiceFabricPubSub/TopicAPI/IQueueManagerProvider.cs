using System.Threading.Tasks;

namespace TopicAPI
{
    public interface IQueueManagerProvider
    {
        void PutAsync(object message, string topic);
        Task<object> GetAsync(string topic);
    }
}
