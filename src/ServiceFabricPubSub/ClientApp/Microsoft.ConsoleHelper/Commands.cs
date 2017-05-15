using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.ConsoleHelper
{
    /// <summary>
    /// Holds a list of commands than can be called by the user
    /// </summary>
    public class Commands
    {
        private readonly List<Tuple<string, Func<Task>>> _commands = new List<Tuple<string, Func<Task>>>();

        public void RegisterCommand(string description, Func<Task> operation)
        {
            if (String.IsNullOrEmpty(description))
                throw new ArgumentException("The description cannot be null");

            _commands.Add(Tuple.Create(description, operation));
        }

        public void Append(Commands other, bool includeLast)
        {
            if (other == null)
                throw new ArgumentException("The commands collection cannot be null");

            _commands.AddRange(other._commands);
            if (!includeLast)
            {
                _commands.RemoveAt(_commands.Count - 1);
            }

        }

        public Func<Task> GetCommand(int commandNumber)
        {
            if (commandNumber<0 || commandNumber >= _commands.Count)
            {
                return null;
            }
            return _commands[commandNumber].Item2;
        }

        public string GetCommandDescription(int commandNumber)
        {
            if (commandNumber < 0 || commandNumber >= _commands.Count)
            {
                throw new Exception("Unknown command " + commandNumber);
            }
            return _commands[commandNumber].Item1;
        }

        public int Count => _commands.Count;
    }
}
