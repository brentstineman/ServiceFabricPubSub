using Microsoft.ConsoleHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ClientApi;

namespace ClientApp
{
    public class PubSubCommands
    {
        #region Tenant Commands
        public static async Task TenantRegisterNew()
        {
            try
            {
                Program.EnsureParam(Program.EnsureConfig.ServiceFabricAdminUri);
                Program.EnsureParam(Program.EnsureConfig.TenantName);
                //Program.EnsureParam(Program.EnsureConfig.AppVersion);

                PubSubClientApi client = new PubSubClientApi(Program.ServiceFabricAdminUri);
                client.Tenants.GetTenant(Program.TenantName, "1.0.0");
              
                Console.WriteLine("Created.");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                throw new CommandFailedException("Web call failed", ex);
            }
            finally
            {
                Program.TenantName = String.Empty;
            }
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
                Program.EnsureParam(Program.EnsureConfig.ServiceFabricUri);
                Program.EnsureParam(Program.EnsureConfig.TenantName);
                Program.EnsureParam(Program.EnsureConfig.TopicName);

                HttpClient client = new HttpClient();
                var response = await client.GetAsync($"{Program.ServiceFabricUri.AbsoluteUri}/api/request/{Program.TenantName}/{Program.TopicName}", HttpCompletionOption.ResponseContentRead);

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
