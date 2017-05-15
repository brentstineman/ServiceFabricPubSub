using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientApp
{
    public class GenericCommands
    {
        public static Task ToggleFlatDisplayCmd()
        {
            return Task.Run(() =>
            {
                Program.FlatDisplay = !Program.FlatDisplay;
            });
        }
       
    }
}
