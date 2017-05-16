using Microsoft.ConsoleHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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
            try
            {
                Program.EnsureParam(Program.EnsureConfig.TenantId);
                Program.EnsureParam(Program.EnsureConfig.TopicId);


                HttpClient client = new HttpClient();
                var response = await client.GetAsync($"{Program.ServiceUri.AbsoluteUri}/api/request/{Program.TenantId}/{Program.TopicId}", HttpCompletionOption.ResponseContentRead);

                response.EnsureSuccessStatusCode();

                Console.WriteLine("Sent");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                throw new CommandFailedException("Web call failed", ex);
            }            
          
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
