using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.ConsoleHelper
{
    public enum ExecutionLevel
    {
        OK,
        Warning,
        Error
    };

    public class ExecutionReport
    {
        public ExecutionLevel m_level;
        public string m_message;

        public ExecutionReport(ExecutionLevel level, string message)
        {
            m_level = level;
            m_message = message;
        }

        public override string ToString()
        {
            return (m_level == ExecutionLevel.OK) ? m_message : string.Format("{0}: {1}", m_level.ToString(), m_message);
        }
    }
}
