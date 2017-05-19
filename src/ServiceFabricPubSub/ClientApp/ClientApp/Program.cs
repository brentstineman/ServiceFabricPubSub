using Microsoft.ConsoleHelper;
using Microsoft.ConsoleHelper.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientApp
{
    public class Program
    {
        private static Groups _groups = new Groups();
        private static Commands _flatCommands = new Commands();
        private static UserInput _userInput = null;
        public static Uri ServiceFabricUri = null;      //router service (etnry point for messages and operatins)
        public static Uri ServiceFabricAdminUri = null; //administration service
        public static string TenantName = null;
        public static string TopicName = null;
        public static string AccessKey = null;
        public static string SubscriberName = null;



        #region Properties
        public static bool FlatDisplay { get; set; }
        #endregion

        [STAThread]
        static void Main(string[] args)
        {
            CheckInputArgs(args);
            SetupCommands();
            _userInput = new UserInput();

            AsyncPump.Run(async delegate
            {
                bool execute = true;
                while (execute)
                {
                    execute = await Run();
                }
            });
            Console.WriteLine("Enter any key to terminate: ");
        }

        private static void CheckInputArgs(string[] args)
        {
            try
            {
                if (args.Length == 1)
                {
                    ServiceFabricUri = new Uri(args[0]);
                }
                if (args.Length == 2)
                {
                    ServiceFabricUri = new Uri(args[0]);
                    ServiceFabricAdminUri = new Uri(args[1]);
                }
                else if (args.Length > 0)
                {
                    throw new ArgumentException();
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Usage: ClientApp.exe [Service_Fabric_Url] [Service_Fabric_Admin_Url]");
                Console.ReadKey();
                System.Environment.Exit(-1);
            }

        }

        static void PrintBanner()
        {
            ConsolePrintHelper.PrintSeparationLine(40);
            Console.WriteLine("Publish/Subscriber Service Fabric Client");
            ConsolePrintHelper.PrintSeparationLine(40);
        }

        /// <summary>
        /// Creates the commands and the groups 
        /// </summary>
        static void SetupCommands()
        {
            var tenantsCmds = new Commands();
            tenantsCmds.RegisterCommand("Register new", PubSubCommands.TenantRegisterNew);
            tenantsCmds.RegisterCommand("Add a Topic", PubSubCommands.TenantCreateTopic);
            tenantsCmds.RegisterCommand("Delete a Topic", PubSubCommands.TenantDeleteTopic);
            tenantsCmds.RegisterCommand("List Topics", PubSubCommands.TenantListTopics);
            tenantsCmds.RegisterCommand("Security Key reset (not working)", PubSubCommands.TenantSecurityKeyReset);
            _groups.AddGroup("Tenant", tenantsCmds);

            var topicCmds = new Commands();
            topicCmds.RegisterCommand("Put message", PubSubCommands.TopicPutMessage);
            topicCmds.RegisterCommand("Add subscriber", PubSubCommands.TopicAddSubscriber);
            topicCmds.RegisterCommand("Delete subscriber", PubSubCommands.TopicDeleteSubscriber);
            topicCmds.RegisterCommand("List subscribers", PubSubCommands.TopicListSubscribers);
            _groups.AddGroup("Topic", topicCmds);

            var subscriberCmds = new Commands();
            subscriberCmds.RegisterCommand("Get message (not implemented yet)", PubSubCommands.SubscriberGetMessage);
            subscriberCmds.RegisterCommand("Get subscriber queue depth (not implemented yet)", PubSubCommands.SubscriberGetQueueDepth);
            subscriberCmds.RegisterCommand("Delete all queue messages (not implemented yet)", PubSubCommands.SubscriberDeleteAllQueuedMessages);
            _groups.AddGroup("Subscriber", subscriberCmds);

            var settingsCmds = new Commands();
            settingsCmds.RegisterCommand("Toggle display mode", GenericCommands.ToggleFlatDisplayCmd);
            _groups.AddGroup("Settings", settingsCmds);

            _flatCommands = _groups.ToFlatCommands();
        }

        /// <summary>
        /// Main entry point for the input and command validation
        /// </summary>
        static async Task<bool> Run()
        {
            Console.Clear();
            Console.ResetColor();
            PrintBanner();

            try
            {
                if (!FlatDisplay)
                    ConsolePrintHelper.PrintCommands(_groups);
                else
                    ConsolePrintHelper.PrintCommands(_flatCommands);

                int? numericCommand;
                bool switchGroup;
                _userInput.GetUserCommandSelection(out switchGroup, out numericCommand);
                if (numericCommand.HasValue)
                {
                    //select and execute the right command

                    int index = numericCommand.Value - 1;
                    Func<Task> operation = null;
                    if (index >= 0)
                    {
                        operation = FlatDisplay ? _flatCommands.GetCommand(index) : _groups.GetCommand(switchGroup, index);
                    }

                    if (operation != null)
                    {
                        ConsolePrintHelper.PrintSeparationLine(80, '-');
                        await operation();
                    }
                    else
                    {
                        Console.WriteLine("Numeric value {0} does not have a valid operation", numericCommand.Value);
                    }
                }
                else
                    Console.WriteLine("Missing command");
            }
            catch (CommandFailedException commandFailed)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"The command failed: {commandFailed.Message}");
                if(commandFailed.InnerException != null)
                    Console.WriteLine("Inner: {0}", commandFailed.InnerException.Message);
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Ooops, something wrong: {0}", ex.Message);
                Console.ReadKey();
            }
            finally
            {
                
            }

            return true;
        }

        /// <summary>
        /// Used to ask for parameters inside a command
        /// </summary>
        /// <param name="param"></param>
        public static void EnsureParam(EnsureConfig param)
        {
            if (ServiceFabricUri == null && (param & EnsureConfig.ServiceFabricUri) == EnsureConfig.ServiceFabricUri)
            {
                string serviceUri = "";
                var serviceUriString = _userInput.EnsureParam(serviceUri, "Service Fabric Uri", forceReEnter: ((EnsureConfig.None & EnsureConfig.ServiceFabricUri) == EnsureConfig.ServiceFabricUri));
                ServiceFabricUri = new Uri(serviceUriString);
            }

            if (ServiceFabricAdminUri == null && (param & EnsureConfig.ServiceFabricAdminUri) == EnsureConfig.ServiceFabricAdminUri)
            {
                string serviceAdminUri = "";
                var serviceAdminUriString = _userInput.EnsureParam(serviceAdminUri, "Service Fabric ADMIN Uri", forceReEnter: ((EnsureConfig.None & EnsureConfig.ServiceFabricAdminUri) == EnsureConfig.ServiceFabricAdminUri));
                ServiceFabricAdminUri = new Uri(serviceAdminUriString);
            }

            if (String.IsNullOrEmpty(TopicName) && (param & EnsureConfig.TopicName) == EnsureConfig.TopicName)
            {
                TopicName = _userInput.EnsureParam(TopicName, "Topic Name", forceReEnter: ((EnsureConfig.None & EnsureConfig.TopicName) == EnsureConfig.TopicName));
            }

            if (String.IsNullOrEmpty(TenantName) && (param & EnsureConfig.TenantName) == EnsureConfig.TenantName)
            {
                TenantName = _userInput.EnsureParam(TenantName, "Tenant Name", forceReEnter: ((EnsureConfig.None & EnsureConfig.TenantName) == EnsureConfig.TenantName));
            }

            if (String.IsNullOrEmpty(AccessKey) && (param & EnsureConfig.AccessKey) == EnsureConfig.AccessKey)
            {
                AccessKey = _userInput.EnsureParam(AccessKey, "Access Key", forceReEnter: ((EnsureConfig.None & EnsureConfig.AccessKey) == EnsureConfig.AccessKey));
            }

            if (String.IsNullOrEmpty(SubscriberName) && (param & EnsureConfig.SubscriberName) == EnsureConfig.SubscriberName)
            {
                SubscriberName = _userInput.EnsureParam(SubscriberName, "Subscriber Name", forceReEnter: ((EnsureConfig.None & EnsureConfig.SubscriberName) == EnsureConfig.SubscriberName));
            }

        }

        [Flags]
        public enum EnsureConfig
        {
            None = 0,
            ServiceFabricUri = 0x1,
            ServiceFabricAdminUri = 0x2,
            TenantName = 0x4,
            TopicName = 0x8,
            AccessKey = 0x16,
            SubscriberName = 0x32,

        }
    }
}
