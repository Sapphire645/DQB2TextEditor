using DQB2TextEditor.InfoReading;
using DQB2TextEditor.Proccessing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace DQB2TextEditor.Linkdata
{
    public class LINKDATA
    {
        public String LINKDATAVersion => version.VersionName;
        public bool Encrypted => version.Encrypted;
        public String LINKDATAPath => LinkdataPath;
        public String[] Languages => version.Languages;
        public byte[] AsianLanguages => version.AsianLanguage;
        public ObservableCollection<Dialogue> Dialogues { get; private set; }
        public ObservableCollection<TextGroup> MenuTexts { get; private set; }

        private LINKDATAVersion version;
        private String LinkdataPath;
        private WeakReference<byte[]> LINKDATABytes;
        //Temp
        private byte[] LINKDATABytesKeep;

        public LINKDATA(string linkdataPath, uint LinkdataSize) {
            var ver = LINKDATAReader.GetPath(LinkdataSize);
            version = new LINKDATAVersion(ver);
            LinkdataPath = linkdataPath;
            ExtractEntries();
        }

        public void KeepLDLoaded(bool store)
        {
            if (store)
                LINKDATABytesKeep = ReadLINKDATA();
            else
                LINKDATABytesKeep = new byte[0];
        }

        private void ExtractEntries()
        {
            byte[] buffer = System.IO.File.ReadAllBytes(LinkdataPath);

            //try to make faster hopefully.
            uint startFlowDataPointer = version.StartFlowData;
            uint startTextDataPointer = version.StartTextData;
            byte LanguageCount = version.LanguageCount;

            Dialogue[] list = new Dialogue[version.DialogueCount];
            byte[] entry = new byte[32];

            //Not using cache but i cant be bothered.
            for (ushort index = 0; index < version.DialogueCount; index++)
            {
                LINKDATAEntry[] TextDataIDXs = new LINKDATAEntry[LanguageCount];
                //Flowdata
                Array.Copy(buffer, (startFlowDataPointer + index) * 32, entry, 0, 32);
                var FlowDataIDX = new LINKDATAEntry((ushort)(startFlowDataPointer + index), entry, FolderType.FlowData);

                //Get all texts
                var pointer = startTextDataPointer + (index * LanguageCount);
                for (int lang = 0; lang < LanguageCount; lang++)
                {
                    Array.Copy(buffer, (pointer + lang)*32, entry, 0, 32);
                    TextDataIDXs[lang] = new LINKDATAEntry((ushort)(pointer + lang), entry, FolderType.TextData);
                }
                //Add to temp list
                list[index] = new Dialogue(FlowDataIDX, TextDataIDXs, index);
            }
            Dialogues = new ObservableCollection<Dialogue>(list);
            List<TextGroup> TList = new List<TextGroup>();
            foreach (var zone in version.IndividualText)
            {
                startTextDataPointer = (uint)zone.Item1;
                var ECount = zone.Item2;

                entry = new byte[32];

                //Not using cache but i cant be bothered.
                for (ushort index = 0; index < ECount; index++)
                {
                    LINKDATAEntry[] TextDataIDXs = new LINKDATAEntry[LanguageCount];
                    //Get all texts
                    var pointer = startTextDataPointer + (index * LanguageCount);
                    for (int lang = 0; lang < LanguageCount; lang++)
                    {
                        Array.Copy(buffer, (pointer + lang) * 32, entry, 0, 32);
                        TextDataIDXs[lang] = new LINKDATAEntry((ushort)(pointer + lang), entry, FolderType.TextData);
                    }
                    //Add to temp list
                    TList.Add(new TextGroup(TextDataIDXs, index));
                }
                
            }
            MenuTexts = new ObservableCollection<TextGroup>(TList);
        }

        private byte[] ReadLINKDATA()
        {
            if (!System.IO.File.Exists(LinkdataPath)) return null;
            String path = LinkdataPath.Substring(0, LinkdataPath.Length - 3) + "BIN";

            if (!System.IO.File.Exists(path)) return null;
            byte[] bin = System.IO.File.ReadAllBytes(path);

            LINKDATABytes = new WeakReference<byte[]>(bin);
            return bin;
        }

        private void WriteLINKDATA(byte[] idx, byte[] bin)
        {
            if (!System.IO.File.Exists(LinkdataPath)) return;
            String path = LinkdataPath.Substring(0, LinkdataPath.Length - 3) + "BIN";
            
            if (!System.IO.File.Exists(path)) return;
            System.IO.File.WriteAllBytes(path, bin);

            System.IO.File.WriteAllBytes(LinkdataPath, idx);
            LINKDATABytes = new WeakReference<byte[]>(bin);
        }

        public byte[] GetEntryBytes(LINKDATAEntry entry) {
            byte[] bin = null;
            if (!(LINKDATABytes != null && LINKDATABytes.TryGetTarget(out bin)))
                bin = ReadLINKDATA();
            //Text File
            UInt64 size = (UInt64)entry.CompressedSize;
            byte[] buffer = new byte[size];
            Array.Copy(bin, (Int64)entry.Offset, buffer, 0, buffer.Length);

            return buffer;
        }

        public void SetEntryBytes(LINKDATAEntry entry, byte[] newData, UInt32 uncompressedSize)
        {
            //System.IO.File.WriteAllBytes("ENTRY-OLD", GetEntryBytes(entry));
            //System.IO.File.WriteAllBytes("ENTRY-NEW", newData);

            //This all needs updating in the future. I just want it to start working rn.
            uint size = uncompressedSize;

            byte[] link_bin = null;
            if (!(LINKDATABytes != null && LINKDATABytes.TryGetTarget(out link_bin)))
                link_bin = ReadLINKDATA();

            byte[] link_idx = System.IO.File.ReadAllBytes(LinkdataPath);

            // offset = after - original
            // original file
            int offset = ((int)entry.CompressedSize + 0x80) / 0x100 * 0x100;
            // after file
            offset = (newData.Length + 0x80) / 0x100 * 0x100 - offset;


            Byte[] bin = new Byte[link_bin.Length + offset];
            Array.Copy(link_bin, 0, bin, 0, (int)entry.Offset);

            Array.Fill<Byte>(link_idx, 0, entry.Index * 32 + 8, 24);
            Array.Copy(BitConverter.GetBytes(size), 0, link_idx, entry.Index * 32 + 8, 4);
            Array.Copy(BitConverter.GetBytes(newData.Length), 0, link_idx, entry.Index * 32 + 16, 4);
            Array.Copy(BitConverter.GetBytes(entry.IsCompressed ? (ulong)1 : (ulong)0), 0, link_idx, entry.Index * 32 + 24, 4);
            Array.Copy(newData, 0, bin, (int)entry.Offset, newData.Length);

            for (int index = entry.Index + 1; index < link_idx.Length / 32; index++)
            {
                var IDXofset = BitConverter.ToUInt64(link_idx, index * 32 + 0);
                var IDXcompressed = BitConverter.ToUInt64(link_idx, index * 32 + 16);
                UInt64 address = IDXofset + (UInt64)offset;
                Array.Copy(BitConverter.GetBytes((UInt64)address), 0, link_idx, index * 32, 8);
                Array.Copy(link_bin, (int)IDXofset, bin, (int)address, (int)IDXcompressed);
            }
            WriteLINKDATA(link_idx, bin);
            ExtractEntries();
        }
    }
}
