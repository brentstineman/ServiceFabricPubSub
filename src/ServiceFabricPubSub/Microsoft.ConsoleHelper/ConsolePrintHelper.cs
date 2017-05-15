using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.ConsoleHelper
{
    public static class ConsolePrintHelper
    {
        public static void PrintCommands(Groups group)
        {
            Console.WriteLine();
            WriteColoredValue("What do you want to do (select ", "numeric", ConsoleColor.Green, " value)?", showEquals: false, newLine: true);
            Commands commands;
            if (group.CurrentGroup != null)
            {
                WriteColoredValue("Current group", group.CurrentGroup.Item1, ConsoleColor.Magenta, newLine: true);
                WriteColoredValue("You can use ", "#1, #2 ... ", ConsoleColor.Green, "to quickly switch to another group", showEquals: false, newLine: true);
                commands = group.CurrentGroup.Item2;
            }
            else
            {
                Console.WriteLine("Select command group:");
                commands = group.TopLevelCommands;
            }
            PrintSeparationLine(30, '-');
            PrintCommandsImpl(commands);
        }

        public static void PrintSeparationLine(int length, char c = '=') {
            StringBuilder sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
                sb.Append(c);
            Console.WriteLine(sb.ToString());
        }

        public static void PrintCommands(Commands commands)
        {
            Console.WriteLine();
            WriteColoredValue("What do you want to do (select ", "numeric", ConsoleColor.Green, " value)?", showEquals: false, newLine: true);
            PrintCommandsImpl(commands);
        }

        public static void WriteColoredStringLine(string text, ConsoleColor color, int coloredChars)
        {
            Console.ForegroundColor = color;
            Console.Write(text.Substring(0, coloredChars));
            Console.ResetColor();
            Console.WriteLine(text.Substring(coloredChars));
        }

        public static void WriteColoredValue(string desc, string param, ConsoleColor color, string restOfLine = null, bool showEquals = true, bool newLine = false)
        {
            Console.Write(desc);
            if (showEquals)
            {
                Console.Write(" = ");
            }

            Console.ForegroundColor = color;
            Console.Write(param);
            Console.ResetColor();
            if (restOfLine != null)
                Console.Write(restOfLine);

            if (newLine)
            {
                Console.WriteLine();
            }
        }

        public static string ReadPassword()
        {
            ConsoleKeyInfo key;
            var password = string.Empty;

            do
            {
                key = Console.ReadKey(true);
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                    {
                        password = password.Substring(0, (password.Length - 1));
                        Console.Write("\b \b");
                    }
                }
            }
            // Stops Receving Keys Once Enter is Pressed
            while (key.Key != ConsoleKey.Enter);

            Console.WriteLine();
            return password;
        }

        private static void PrintCommandsImpl(Commands commands)
        {
            for (int i = 0; i < commands.Count; i++)
            {
                var numericSize = i < 9 ? 1 : ((i < 99) ? 2 : 3);
                var align = i < 9 ? " " : "";
                WriteColoredStringLine(string.Format("{0} {1} {2}", i + 1, align, commands.GetCommandDescription(i)), ConsoleColor.Green, numericSize);
            }
            Console.WriteLine();
        }
    }
}
