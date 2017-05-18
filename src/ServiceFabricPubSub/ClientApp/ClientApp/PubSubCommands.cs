using Microsoft.ConsoleHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ClientApi;
using ClientApi.Admin;
using ClientApi.Router;
using Newtonsoft.Json;

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

                PubSubAdminApi client = new PubSubAdminApi(Program.ServiceFabricAdminUri);
                Program.AccessKey = client.Tenants.CreateTenant(Program.TenantName, "1.0.0");
              
                Console.WriteLine($"Tenant '{Program.TenantName}' created.");
                Console.WriteLine($"Access key: {Program.AccessKey}");

                Console.ReadKey();
            }
            catch (Exception ex)
            {
                throw new CommandFailedException("Web call failed", ex);
            }
            finally
            {
                Program.TenantName = String.Empty;
                Program.TopicName = String.Empty;      
                Program.AccessKey = String.Empty;
            }
        }

        public static async Task TenantSecurityKeyReset()
        {
            try
            {
                Program.EnsureParam(Program.EnsureConfig.ServiceFabricAdminUri);
                Program.EnsureParam(Program.EnsureConfig.TenantName);
                Program.EnsureParam(Program.EnsureConfig.AccessKey);

                //TODO add reset security
                PubSubAdminApi client = new PubSubAdminApi(Program.ServiceFabricAdminUri);
                var result = await client.Topics.DeleteTopicWithHttpMessagesAsync(Program.TenantName, Program.TopicName, AuthenticationHeader());

                Console.WriteLine("Deleted");

                Console.ReadKey();
            }
            catch (Exception ex)
            {
                throw new CommandFailedException("Web call failed", ex);
            }
            finally
            {
                Program.TenantName = String.Empty;
                Program.TopicName = String.Empty;
            }
        }

        public static async Task TenantCreateTopic()
        {
            try
            {
                Program.EnsureParam(Program.EnsureConfig.ServiceFabricAdminUri);
                Program.EnsureParam(Program.EnsureConfig.TenantName);
                Program.EnsureParam(Program.EnsureConfig.TopicName);
                Program.EnsureParam(Program.EnsureConfig.AccessKey);

                PubSubAdminApi client = new PubSubAdminApi(Program.ServiceFabricAdminUri);
                var result = await client.Topics.CreateTopicWithHttpMessagesAsync(Program.TenantName,Program.TopicName, AuthenticationHeader());

                Console.WriteLine($"Topic '{Program.TopicName}' created.");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                throw new CommandFailedException("Web call failed", ex);
            }
            finally
            {
                Program.TenantName = String.Empty;
                Program.TopicName = String.Empty;
                Program.AccessKey = String.Empty;
            }
        }

        public static async Task TenantDeleteTopic()
        {
            try
            {
                Program.EnsureParam(Program.EnsureConfig.ServiceFabricAdminUri);
                Program.EnsureParam(Program.EnsureConfig.TenantName);
                Program.EnsureParam(Program.EnsureConfig.TopicName);
                Program.EnsureParam(Program.EnsureConfig.AccessKey);

                PubSubAdminApi client = new PubSubAdminApi(Program.ServiceFabricAdminUri);
                var result = await client.Topics.DeleteTopicWithHttpMessagesAsync(Program.TenantName, Program.TopicName, AuthenticationHeader());

                Console.WriteLine("Deleted");

                Console.ReadKey();
            }
            catch (Exception ex)
            {
                throw new CommandFailedException("Web call failed", ex);
            }
            finally
            {
                Program.TenantName = String.Empty;
                Program.TopicName = String.Empty;
                Program.AccessKey = String.Empty;
            }
        }

        public static async Task TenantListTopics()
        {
            try
            {
                Program.EnsureParam(Program.EnsureConfig.ServiceFabricAdminUri);
                Program.EnsureParam(Program.EnsureConfig.TenantName);
                Program.EnsureParam(Program.EnsureConfig.AccessKey);

                PubSubAdminApi client = new PubSubAdminApi(Program.ServiceFabricAdminUri);
                var result = await client.Topics.GetTopicsWithHttpMessagesAsync(Program.TenantName, AuthenticationHeader());

                List<string> topics = JsonConvert.DeserializeObject<List<string>>(result.Body.ToString());

                Console.WriteLine("Topics");
                Console.WriteLine("-------------------");

                foreach (string t in topics)
                    Console.WriteLine(t);

                Console.ReadKey();
            }
            catch (Exception ex)
            {
                throw new CommandFailedException("Web call failed", ex);
            }
            finally
            {
                Program.TenantName = String.Empty;
                Program.TopicName = String.Empty;
            }
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

        private static Dictionary<string, List<string>> AuthenticationHeader() {
            Dictionary<string, List<string>> customHeaders = new Dictionary<string, List<string>>(1);
            var authz = new List<string>();
            authz.Add(Program.AccessKey);
            customHeaders.Add("x-request-key", authz);
            return customHeaders;
        }
    }
}
