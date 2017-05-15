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
            //EnsureBasicParams(EnsureExtras.Azure);
            Console.WriteLine("TenantSecurityKeyReset");
            throw new ArgumentException();
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
        #endregion

        #region Subscriber Commands

        #endregion
    }
}
