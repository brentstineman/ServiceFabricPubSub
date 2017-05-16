using Microsoft.ConsoleHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientApp
{
    public class PubSubCommands
    {
        #region Tenant Commands
        public static async Task TenantRegisterNew()
        {
            //EnsureBasicParams(EnsureExtras.Azure);
        }
        public static async Task TenantSecurityKeyReset()
        {         
        }
        public static async Task TenantAddTopic()
        {
        }

        public static async Task TenantDeleteTopic()
        {
        }
        public static async Task TenantListTopics()
        {
        }
        #endregion

        #region Topic Commands
        public static async Task TopicPutMessage()
        {
            throw new CommandFailedException("Failed");
        }
        public static async Task TopicAddSubscriber()
        {
            //EnsureBasicParams(EnsureExtras.Azure);
        }
        public static async Task TopicDeleteSubscriber()
        {
            //EnsureBasicParams(EnsureExtras.Azure);
        }

        public static async Task TopicListSubscribers()
        {
            //EnsureBasicParams(EnsureExtras.Azure);
        }
        #endregion

        #region Subscriber Commands
        public static async Task SubscriberGetMessage()
        {
            //EnsureBasicParams(EnsureExtras.Azure);
        }
        public static async Task SubscriberGetQueueDepth()
        {
            //EnsureBasicParams(EnsureExtras.Azure);
        }

        public static async Task SubscriberDeleteAllQueuedMessages()
        {
            //EnsureBasicParams(EnsureExtras.Azure);
        }
        #endregion
    }
}
