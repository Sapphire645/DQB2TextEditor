using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DQB2TextEditor.Linkdata
{
    public class LINKDATAEntry
    {
        public UInt64 Offset { get; set; }
        public UInt64 UncompressedSize { get; set; }
        public UInt64 CompressedSize { get; set; }
        public bool IsCompressed { get; set; }
        public ushort Index { get; private set; }
        internal LDFolder LINKDATAData { get; private set; }
        public LINKDATAEntry(ushort index, byte[] bytes)
        {
            Index = index;
            Offset = BitConverter.ToUInt64(bytes, 0);
            UncompressedSize = BitConverter.ToUInt64(bytes, 8);
            CompressedSize = BitConverter.ToUInt64(bytes, 16);
            IsCompressed = BitConverter.ToUInt64(bytes, 24) > 0 ? true : false;

            LINKDATAData = new LDFolder(this);
        }
    }
}
