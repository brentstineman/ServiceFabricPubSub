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
using PubSubDotnetSDK;

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
                Program.AccessKey = String.Empty;
            }
        }
        #endregion

        #region Topic Commands
        public static async Task TopicPutMessage()
        {
            //this goes to the router
            try
            {
                Program.EnsureParam(Program.EnsureConfig.ServiceFabricAdminUri);
                Program.EnsureParam(Program.EnsureConfig.TenantName);
                Program.EnsureParam(Program.EnsureConfig.TopicName);
                Program.EnsureParam(Program.EnsureConfig.AccessKey);

                Console.WriteLine("Type your message");
                var message = Console.ReadLine();

                PubSubClientApi client = new PubSubClientApi(Program.ServiceFabricUri);
                var result = await client.Request.PostWithHttpMessagesAsync(Program.TenantName, Program.TopicName, message, AuthenticationHeader());

                Console.WriteLine("Message sent.");
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

        public static async Task TopicAddSubscriber()
        {
            try
            {
                Program.EnsureParam(Program.EnsureConfig.ServiceFabricAdminUri);
                Program.EnsureParam(Program.EnsureConfig.TenantName);
                Program.EnsureParam(Program.EnsureConfig.TopicName);
                Program.EnsureParam(Program.EnsureConfig.SubscriberName);
                Program.EnsureParam(Program.EnsureConfig.AccessKey);
         
                PubSubAdminApi client = new PubSubAdminApi(Program.ServiceFabricAdminUri);
                var result = await client.Subscriber.AddSubscriberWithHttpMessagesAsync(Program.TenantName, Program.TopicName, Program.SubscriberName, AuthenticationHeader());

                Console.WriteLine($"Subscriber '{Program.SubscriberName}' created.");
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
                Program.SubscriberName = String.Empty;
            }
        }
        public static async Task TopicDeleteSubscriber()
        {
            try
            {
                Program.EnsureParam(Program.EnsureConfig.ServiceFabricAdminUri);
                Program.EnsureParam(Program.EnsureConfig.TenantName);
                Program.EnsureParam(Program.EnsureConfig.TopicName);
                Program.EnsureParam(Program.EnsureConfig.SubscriberName);
                Program.EnsureParam(Program.EnsureConfig.AccessKey);

                PubSubAdminApi client = new PubSubAdminApi(Program.ServiceFabricAdminUri);
                var result = await client.Subscriber.DeleteSubscriberWithHttpMessagesAsync(Program.TenantName, Program.TopicName, Program.SubscriberName, AuthenticationHeader());

                Console.WriteLine($"Subscriber '{Program.SubscriberName}' deleted.");
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
                Program.SubscriberName = String.Empty;
            }
        }

        public static async Task TopicListSubscribers()
        {
            try
            {
                Program.EnsureParam(Program.EnsureConfig.ServiceFabricAdminUri);
                Program.EnsureParam(Program.EnsureConfig.TenantName);
                Program.EnsureParam(Program.EnsureConfig.TopicName);
                Program.EnsureParam(Program.EnsureConfig.AccessKey);

                PubSubAdminApi client = new PubSubAdminApi(Program.ServiceFabricAdminUri);
                var result = await client.Subscriber.GetSubscribersWithHttpMessagesAsync(Program.TenantName, Program.TopicName, AuthenticationHeader());

                List<string> subscribers = JsonConvert.DeserializeObject<List<string>>(result.Body.ToString());

                Console.WriteLine("Subscribers");
                Console.WriteLine("-------------------");

                foreach (string s in subscribers)
                    Console.WriteLine(s);

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
                Program.SubscriberName = String.Empty;
            }
        }
        #endregion

        #region Subscriber Commands
        public static async Task SubscriberGetMessage()
        {
            try
            {
                Program.EnsureParam(Program.EnsureConfig.ServiceFabricAdminUri);
                Program.EnsureParam(Program.EnsureConfig.TenantName);
                Program.EnsureParam(Program.EnsureConfig.TopicName);
                Program.EnsureParam(Program.EnsureConfig.SubscriberName);
                Program.EnsureParam(Program.EnsureConfig.AccessKey);

                PubSubClientApi client = new PubSubClientApi(Program.ServiceFabricUri);
                dynamic result = await client.Request.GetWithHttpMessagesAsync(Program.TenantName, Program.TopicName, Program.SubscriberName, AuthenticationHeader());

                Console.WriteLine("Message: " + result.Body.Message);
          
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
                Program.SubscriberName = String.Empty;
            }
        }
        public static async Task SubscriberGetQueueDepth()
        {
          
        }

        public static async Task SubscriberDeleteAllQueuedMessages()
        {

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
