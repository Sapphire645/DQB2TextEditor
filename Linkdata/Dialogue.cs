using DQB2TextEditor.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DQB2TextEditor.Linkdata
{
    public class Dialogue : TextGroup
    {
        private LINKDATAEntry[] _TextDataFiles;
        private LINKDATAEntry _FlowDataFile;
        private ushort _index;
        public ushort Index => _index;

        public ushort FlowDataIndex => _FlowDataFile.Index;
        public UInt64 FlowDataOffset => _FlowDataFile.Offset;
        public UInt64 FlowDataUncompressedSize => _FlowDataFile.UncompressedSize;
        public UInt64 FlowDataCompressedSize => _FlowDataFile.CompressedSize;

        public ushort TextDataIndex => _TextDataFiles[ViewModel._currentLanguage].Index;
        public UInt64 TextDataOffset => _TextDataFiles[ViewModel._currentLanguage].Offset;
        public UInt64 TextDataUncompressedSize => _TextDataFiles[ViewModel._currentLanguage].UncompressedSize;
        public UInt64 TextDataCompressedSize => _TextDataFiles[ViewModel._currentLanguage].CompressedSize;

        public Dialogue(LINKDATAEntry flowDataFile, LINKDATAEntry[] textDataFiles, ushort index)
        {
            _FlowDataFile = flowDataFile;
            _TextDataFiles = textDataFiles;
            _index = index;
        }

    }
}
