using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.ConsoleHelper
{
    /// <summary>
    /// Groups of commands for user input
    /// </summary>
    public class Groups
    {
        public Tuple<string, Commands> CurrentGroup { get; private set; }

        public Commands TopLevelCommands { get; private set; }

        private List<Tuple<string, Commands>> m_commandGroups { get; set; }

        public Groups()
        {
            m_commandGroups = new List<Tuple<string, Commands>>();
            TopLevelCommands = new Commands();
        }
        public void AddGroup(string name, Commands commands)
        {
            commands.RegisterCommand("Exit group", ExitGroup);
            m_commandGroups.Add(new Tuple<string, Commands>(name, commands));
            var groupNum = m_commandGroups.Count - 1;
            TopLevelCommands.RegisterCommand(name, () => SetGroup(groupNum));
        }

        public Func<Task> GetCommand(bool switchGroup, int index)
        {
            if (!switchGroup && CurrentGroup != null)
                return CurrentGroup.Item2.GetCommand(index);

            return TopLevelCommands.GetCommand(index);
        }

        public Commands ToFlatCommands()
        {
            var commands = new Commands();
            foreach (var commandGroup in m_commandGroups)
            {
                commands.Append(commandGroup.Item2, includeLast: false);
            }
            return commands;
        }

        private Task ExitGroup()
        {
            return Task.Run(() => {
                CurrentGroup = null;
            });
        }

        private Task SetGroup(int group)
        {
            return Task.Run(() => {
                if (m_commandGroups.Count > group)
                {
                    CurrentGroup = m_commandGroups[group];
                }
            });
        }
    }

}
