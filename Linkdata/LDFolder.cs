using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DQB2TextEditor.Linkdata
{
    internal class LDFolder
    {
        public List<LDFile> Files { get; private set; } = new List<LDFile>();
        public UInt32 SplitSize { get; private set; }
        public UInt32 UncompressedSize { get; private set; }
        public LINKDATAEntry Entry { get; private set; }

        public LDFolder(LINKDATAEntry Entry)
        {
            this.Entry = Entry;
        }
    }
}
