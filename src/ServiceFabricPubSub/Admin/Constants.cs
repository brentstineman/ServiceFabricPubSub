using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading.Tasks;

namespace Admin
{
    public class Constants
    {
        public const string TOPIC_SERVICE_TYPE_NAME = "TopicServiceType";
        public const string SUBSCRIBER_SERVICE_TYPE_NAME = "SubscriberServiceType";

        public const string KEY1 = "key1";
        public const string KEY2 = "key2";
        public const string COLLECTION_KEYS = "keys";
        public const string COLLECTION_REMOVE_SUBSCRIPTIONS = "removesubs";

       
    }

    public static class AppHelpers
    {
        public static bool IsTopic(this Uri uri, string topic)
        {
            return uri.ToString().Contains($"/topics/{topic}");
        }

        public static Uri CreateSubscriptionUri(this StatefulServiceContext serviceContext,  string topicName, string subscriptionName)
        {
            return new Uri($"{serviceContext.CodePackageActivationContext.ApplicationName}/topics/{topicName}/{subscriptionName}");
        }

        public static Uri CreateTopicUri(this StatefulServiceContext serviceContext, string topicName)
        {
            return new Uri($"{serviceContext.CodePackageActivationContext.ApplicationName}/topics/{topicName}");
        }

        public static async Task<IReliableDictionary<string, string>> GetRemoveSubscriptionsDictionary(this IReliableStateManager stateManager)
        {
            return await stateManager.GetOrAddAsync<IReliableDictionary<string, string>>(Constants.COLLECTION_REMOVE_SUBSCRIPTIONS);
        }
    }
}
