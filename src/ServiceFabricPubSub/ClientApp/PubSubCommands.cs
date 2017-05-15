using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientApp
{
    public class PubSubCommands
    {
        public static async Task HelloCmd()
        {
            //EnsureBasicParams(EnsureExtras.Azure);

            Console.ForegroundColor = ConsoleColor.Cyan;

            Console.WriteLine("Ciao");
        }
    }
}
