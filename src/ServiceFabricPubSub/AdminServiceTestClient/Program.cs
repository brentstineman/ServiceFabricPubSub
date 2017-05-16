using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using AdminService;
using Microsoft.ServiceFabric.Services.Client;

namespace AdminServiceTestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.Run(async () =>
            {

                var partionKey = new ServicePartitionKey(0);

                IAdminService adminServiceClient = ServiceProxy.Create<IAdminService>(
                    new Uri("fabric:/TenantApplication/AdminService"), partionKey);
                string key = await adminServiceClient.GetKey1();

                Console.WriteLine($"Key: {key}");

                Console.WriteLine("Hit any key to continue");
                Console.ReadLine();

                await adminServiceClient.CreateNewTopic("topic1");
                await adminServiceClient.CreateNewTopic("topic2");

                Console.WriteLine("Hit any key to continue");

            }).GetAwaiter().GetResult();
        }
    }
}
