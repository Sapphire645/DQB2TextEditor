using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DQB2TextEditor.Linkdata.LineEntry
{
    internal class LineEntryFabric
    {
        public static FlowDataLine CreateLineEntry(byte[] data, string line)
        {

            return new FlowDataLine(data, line);
        }
    }
}
