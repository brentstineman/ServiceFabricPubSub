using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.ConsoleHelper
{
    /// <summary>
    /// Utilities for getting user insertion. To be overritten for processing scripts
    /// </summary>
    public class UserInput
    {
        public virtual int? EnsureIntParam(int? param, string desc, bool onlyFillIfEmpty = false, bool forceReEnter = false)
        {
            bool available = param.HasValue;
            if (onlyFillIfEmpty && available)
            {
                return param;
            }

            if (available)
            {
                PrintHelper.WriteColoredValue(desc, param.Value.ToString(), ConsoleColor.Magenta, forceReEnter ? ". Re-Enter same, or new int value:" : ". Press enter to use, or give new int value:");
            }
            else
            {
                Console.Write(desc + " is required. Enter int value:");
            }

            var entered = Console.ReadLine();
            int val;
            if (!string.IsNullOrWhiteSpace(entered))
            {
                if (!Int32.TryParse(entered, out val))
                {
                    Console.WriteLine("illegal int value:[" + entered + "]");
                    return null;
                }
                param = val;
            }
            return param;
        }

        public virtual string EnsureParam(string param, string desc, bool onlyFillIfEmpty = false, bool forceReEnter = false, bool isPassword = false)
        {
            bool available = !string.IsNullOrWhiteSpace(param);
            if (onlyFillIfEmpty && available)
            {
                return param;
            }

            if (available)
            {
                PrintHelper.WriteColoredValue(desc, param, ConsoleColor.Magenta, forceReEnter ? ". Re-Enter same, or new value:" : ". Press enter to use, or give new value:");
            }
            else
            {
                Console.Write(desc + " is required. Enter value:");
            }

            var entered = isPassword ? PrintHelper.ReadPassword() : Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(entered))
            {
                param = entered;
            }

            return param;
        }

        public virtual string EnterOptionalParam(string desc, string skipResultDescription)
        {
            PrintHelper.WriteColoredValue(desc + " (optional). Enter value (or press Enter to ", skipResultDescription, ConsoleColor.Magenta, "):");

            var entered = Console.ReadLine();
            Console.WriteLine();
            if (!string.IsNullOrWhiteSpace(entered))
            {
                return entered;
            }

            return null;
        }
        public virtual string ManageCachedParam(string param, string desc, bool forceReset = false)
        {
            if (forceReset)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(param))
            {
                PrintHelper.WriteColoredValue(desc, param, ConsoleColor.Magenta, ". Enter 'Y': to Reset, 'A': to assign, Q: to Quit, Any another key to skip:");
            }
            else
            {
                PrintHelper.WriteColoredValue(desc, param, ConsoleColor.Magenta, ". Enter 'A': to assign, Q: to Quit, Any another key to skip:");
            }
            var ch = Char.ToUpper(Console.ReadKey().KeyChar);
            Console.WriteLine();
            switch (ch)
            {
                case 'Y':
                    return null;
                case 'A':
                    param = EnsureParam(null, desc);
                    break;
                case 'Q':
                    throw new Exception(string.Format("Quit managing cache when on '{0}', value ={1}", desc, param));
                default:
                    break;
            }
            return param;
        }

        public virtual void GetUserCommandSelection(out bool switchGroup, out int? numericCommand)
        {
            numericCommand = null;
            while (true)
            {
                var command = Console.ReadLine();
                if (string.IsNullOrEmpty(command))
                {
                    Console.WriteLine("No input. Try again");
                    continue;
                }

                int val;
                if (command[0] == '#')
                {
                    switchGroup = true;
                    command = command.TrimStart('#');
                }
                else
                {
                    switchGroup = false;
                }

                if (int.TryParse(command, out val))
                {
                    numericCommand = val;
                    return;
                }

                Console.WriteLine("Illegal input. Try again");
            }
        }
    }

}
