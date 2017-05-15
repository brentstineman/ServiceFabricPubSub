using Microsoft.ConsoleHelper;
using Microsoft.ConsoleHelper.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientApp
{
    class Program
    {
        static Groups groups = new Groups();
        static Commands flatCommands = new Commands();
        static bool flatDisplay = false;
        static UserInput userInput = null;

        [STAThread]
        static void Main(string[] args)
        {

            SetupCommands();
            userInput = new UserInput();

            AsyncPump.Run(async delegate
            {
                bool execute = true;
                while (execute)
                {
                    execute = await Run();
                }
            });
            Console.WriteLine("Enter any key to terminate: ");
            Console.ReadKey(true);
        }

        static void SetupCommands()
        {
            var commands = new Commands();
            commands.RegisterCommand("Test", PubSubCommands.HelloCmd);
            groups.AddGroup("Test groups", commands);

            flatCommands = groups.ToFlatCommands();
        }

        static async Task<bool> Run()
        {
            Console.ResetColor();
            try
            {
                if (!flatDisplay)
                    PrintHelper.PrintCommands(groups);
                else
                    PrintHelper.PrintCommands(flatCommands);

                int? numericCommand;
                bool switchGroup;
                userInput.GetUserCommandSelection(out switchGroup, out numericCommand);
                if (numericCommand.HasValue)
                {
                    int index = numericCommand.Value - 1;
                    Func<Task> operation = null;
                    if (index >= 0)
                    {
                        operation = flatDisplay ? flatCommands.GetCommand(index) : groups.GetCommand(switchGroup, index);
                    }

                    if (operation != null)
                    {
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
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Ooops, something wrong: {0}", ex.Message);
                Console.WriteLine();
            }

            return true;
        }
     }
}
