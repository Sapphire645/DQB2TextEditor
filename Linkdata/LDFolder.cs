using DQB2TextEditor.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace DQB2TextEditor.Linkdata
{
    internal class LDFolder
    {
        public UInt32 SplitSize { get; private set; }
        public UInt32 UncompressedSize { get; private set; }

        private UInt32 FileCount;
        public LINKDATAEntry Entry { get; private set; }

        private WeakReference<LDFile[]> _Files;
        protected virtual FolderType Type => FolderType.Unknown;
        public LDFile[] Files
        {
            get
            {
                if (!(_Files != null && _Files.TryGetTarget(out LDFile[] list)))
                {
                    byte[] bin = ViewModel.linkdata.GetEntryBytes(Entry);

                    if (_Files == null)
                    {
                        SplitSize = BitConverter.ToUInt32(bin, 0);
                        FileCount = BitConverter.ToUInt32(bin, 4);
                        UncompressedSize = BitConverter.ToUInt32(bin, 8);
                    }
                    list = new LDFile[FileCount];

                    UInt32 offset = 0x0C + FileCount * 4;
                    for (int count = 0; count < FileCount; count++)
                    {
                        if (offset % 0x80 != 0) offset = (offset / 0x80 + 1) * 0x80;
                        UInt32 size = BitConverter.ToUInt32(bin, 0x0C + count * 4);
                        byte[] binary = new byte[size];

                        Array.Copy(bin, offset + 4, binary, 0, size);
                        LDFile file = null;
                        switch (Type)
                        {
                            case FolderType.TextData:
                                file = new LDFile_TextData(binary);
                                break;
                            case FolderType.FlowData:
                                file = new LDFile(binary, false);
                                break;
                            case FolderType.Unknown:
                                file = new LDFile(binary, Entry.IsCompressed);
                                break;
                        }
                        
                        list[count] = file;
                        offset += size;
                    }
                }
                return list;
            }
        }

        public LDFolder(LINKDATAEntry Entry)
        {
            this.Entry = Entry;
        }
    }
}
