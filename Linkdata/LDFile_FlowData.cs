using DQB2TextEditor.Linkdata.LineEntry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DQB2TextEditor.Linkdata
{
    internal class LDFile_FlowData : LDFile
    {
        protected override FolderType Type => FolderType.FlowData;
        private readonly ushort EntrySize = 0x34;
        public byte[][] Flows
        {
            get
            {
                if (_flows == null) ExtractData();
                return _flows;
            }
        }

        private byte[][] _flows = null;


        public LDFile_FlowData(LINKDATAEntry Entry) : base(Entry)
        {
        }

        private void ExtractData()
        {
            try
            {
                byte[] uncompressData = uncompressedData;
                //Should be easy I reckon.
                //The size of an entry is...
                int flowcount = uncompressData.Length / EntrySize;

                _flows = new byte[flowcount][];
                for (int i = 0; i < flowcount; i++)
                {
                    _flows[i] = new byte[EntrySize];
                    Array.Copy(uncompressData, i * EntrySize, _flows[i], 0, EntrySize);
                }
            }
            catch
            {
                return;
            }
        }
    }
            
}
