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

namespace DQB2TextEditor.Linkdata
{
    public class LINKDATA
    {
        public String LINKDATAVersion => version.VersionName;
        public String LINKDATAPath => LinkdataPath;
        public String[] Languages => version.Languages;
        public ObservableCollection<Dialogue> Dialogues { get; private set; }
        public ObservableCollection<TextGroup> MenuTexts { get; private set; }

        private LINKDATAVersion version;
        private String LinkdataPath;
        private WeakReference<byte[]> LINKDATABytes;

        public LINKDATA(string linkdataPath, uint LinkdataSize) {
            var ver = LINKDATAReader.GetPath(LinkdataSize);
            version = new LINKDATAVersion(ver);
            LinkdataPath = linkdataPath;
            ExtractEntries();
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
    }
}
