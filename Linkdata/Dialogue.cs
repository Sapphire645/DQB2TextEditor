using DQB2TextEditor.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DQB2TextEditor.Linkdata
{
    public class Dialogue : TextGroup
    {
        private LINKDATAEntry _FlowDataFile;
        public ushort FlowDataIndex => _FlowDataFile.Index;
        public UInt64 FlowDataOffset => _FlowDataFile.Offset;
        public UInt64 FlowDataUncompressedSize => _FlowDataFile.UncompressedSize;
        public UInt64 FlowDataCompressedSize => _FlowDataFile.CompressedSize;

        public Dialogue(LINKDATAEntry flowDataFile, LINKDATAEntry[] textDataFiles, ushort index) : base(textDataFiles, index)
        {
            _FlowDataFile = flowDataFile;
        }




    }
}
