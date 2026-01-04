using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DQB2TextEditor.InfoReading
{
    public class CommandInfo
    {
        public ushort Command { get; private set; }
        public string Name;
        public string Added;
        public string NameFull => Added + " " + Name;
        public string[] Arguments = new string[11];

        public CommandInfo(ushort command, string nameFull)
        {
            Command = command;
            String[] var = nameFull.Split(" ");
            if (var.Length == 2)
            {
                Added = var[0];
                Name = var[1].Trim();
            }
            else
            {
                Added = "";
                Name = var[0].Trim();
            }
        }
    }
}
