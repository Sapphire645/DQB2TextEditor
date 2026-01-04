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
    internal class LDFile
    {
        public UInt32 SplitSize { get; private set; }
        public UInt32 UncompressedSize { get; private set; }

        private UInt32 FileCount;
        public LINKDATAEntry Entry { get; private set; }

        private WeakReference<LDFileChunk[]> _Files;
        protected virtual FolderType Type => FolderType.Unknown;
        protected LDFileChunk[] Files
        {
            get
            {
                if (!(_Files != null && _Files.TryGetTarget(out LDFileChunk[] list)))
                {
                    try
                    {
                        byte[] bin = ViewModel.linkdata.GetEntryBytes(Entry);

                        if (Entry.IsCompressed)
                        {
                            if (_Files == null)
                            {
                                SplitSize = BitConverter.ToUInt32(bin, 0);
                                FileCount = BitConverter.ToUInt32(bin, 4);
                                UncompressedSize = BitConverter.ToUInt32(bin, 8);
                            }
                            list = new LDFileChunk[FileCount];

                            UInt32 offset = 0x0C + FileCount * 4;
                            for (int count = 0; count < FileCount; count++)
                            {
                                if (offset % 0x80 != 0) offset = (offset / 0x80 + 1) * 0x80;
                                UInt32 size = BitConverter.ToUInt32(bin, 0x0C + count * 4);
                                byte[] binary = new Byte[size];

                                if (bin.Length < offset + 4 + size)
                                {
                                    size = (UInt32)(bin.Length - offset - 4); //Crashes sometimes, not sure why....
                                }
                                Array.Copy(bin, offset + 4, binary, 0, size);
                                LDFileChunk file = null;
                                switch (Type)
                                {
                                    case FolderType.TextData:
                                        file = new LDFileChunk(binary, true);
                                        break;
                                    case FolderType.Unknown:
                                        file = new LDFileChunk(binary, Entry.IsCompressed);
                                        break;
                                }
                                list[count] = file;
                                offset += size;
                            }
                        }
                        else
                        {
                            list = new LDFileChunk[1];

                            switch (Type)
                            {
                                case FolderType.FlowData:
                                    list[0] = new LDFileChunk(bin, false);
                                    break;
                                case FolderType.Unknown:
                                    list[0] = new LDFileChunk(bin, Entry.IsCompressed);
                                    break;
                            }
                        }
                        _Files = new WeakReference<LDFileChunk[]>(list);
                        return list;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error extracting files {Entry.Index}: {ex.Message}");
                    }
                    return new LDFileChunk[1];
                }
                return list;
            }
            set
            {
                _Files = new WeakReference<LDFileChunk[]>(value);
                using (MemoryStream ms = new MemoryStream())
                {
                    // split size.
                    ms.Write(BitConverter.GetBytes(SplitSize), 0, 4);
                    // split count.
                    ms.Write(BitConverter.GetBytes(FileCount), 0, 4);
                    // unpack file size.
                    ms.Write(BitConverter.GetBytes(UncompressedSize), 0, 4);
                    // chunk size.
                    foreach (LDFileChunk fil in value)
                    {
                        ms.Write(BitConverter.GetBytes(fil.CompressedSize + 4), 0, sizeof(UInt32));
                    }
                    // padding.
                    int count = 0x80 - ((int)ms.Length % 0x80);
                    if (count == 0x80) count = 0;
                    for (int index = 0; index < count; index++)
                    {
                        ms.WriteByte(0);
                    }
                    foreach (LDFileChunk fil in value)
                    {
                        ms.Write(BitConverter.GetBytes(fil.CompressedSize), 0, sizeof(UInt32));
                        ms.Write(fil.RawData, 0, (int)fil.CompressedSize);
                    }
                    // padding.
                    count = 0x80 - ((int)ms.Length % 0x80);
                    if (count == 0x80) count = 0;
                    for (int index = 0; index < count; index++)
                    {
                        ms.WriteByte(0);
                    }
                    ViewModel.linkdata.SetEntryBytes(Entry, ms.ToArray(), UncompressedSize);
                }

            }
        }

        protected byte[] uncompressedData
        {
            get
            {
                var myFiles = Files;
                if (myFiles == null || myFiles.Length == 0) return new byte[0];
                using (MemoryStream ms = new MemoryStream())
                {
                    foreach (var file in myFiles)
                    {
                        if (file == null) continue;
                        var ud = file.uncompressedData;
                        ms.Write(ud, 0, ud.Length);
                    }
                    return ms.ToArray();
                }
            }
        }

        protected void UpdateFile(byte[] newUncompressedData)
        {
            //Copied Turtle-Insect code. I tried to do it on my own but I just messed everything up.
            Int32 packCount = (int)((newUncompressedData.Length + SplitSize - 1) / SplitSize);

            LDFileChunk[] file = new LDFileChunk[packCount];
            // For each chunk
            for (int pack = 0; pack < packCount; pack++)
            {
                int length = (int)SplitSize;
                if (pack + 1 == packCount) length = (int)(newUncompressedData.Length % SplitSize);
                Byte[] tmp = new Byte[length];
                Array.Copy(newUncompressedData, SplitSize * pack, tmp, 0, tmp.Length);

                switch (Type)
                {
                    case FolderType.TextData:
                        file[pack] = new LDFileChunk(true, tmp);
                        break;
                    case FolderType.FlowData:
                        file[pack] = new LDFileChunk(false, tmp);
                        break;
                    case FolderType.Unknown:
                        file[pack] = new LDFileChunk(Entry.IsCompressed, tmp);
                        break;
                }
            }
            FileCount = (uint)packCount;
            UncompressedSize = (UInt32)newUncompressedData.Length;
            Files = file;
        }

        public LDFile(LINKDATAEntry Entry)
        {
            this.Entry = Entry;
        }

        public void SaveUncompressedFileData(String path)
        {
            System.IO.File.WriteAllBytes(path, uncompressedData);
        }
        //public void SaveCompressedFolderData(String path)
        //{
        //    System.IO.File.WriteAllBytes(path, _data);
        //}

    }
}
