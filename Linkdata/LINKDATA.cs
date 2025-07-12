using DQB2TextEditor.InfoReading;
using DQB2TextEditor.Proccessing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DQB2TextEditor.Linkdata
{
    public class LINKDATA
    {
        private LINKDATAVersion version;
        private String LinkdataPath;

        public ObservableCollection<Dialogue> Dialogues { get; private set; }

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
                var FlowDataIDX = new LINKDATAEntry((ushort)(startFlowDataPointer + index), entry);

                //Get all texts
                var pointer = startTextDataPointer + (index * LanguageCount);
                for (int lang = 0; lang < LanguageCount; lang++)
                {
                    Array.Copy(buffer, (pointer + lang)*32, entry, 0, 32);
                    TextDataIDXs[lang] = new LINKDATAEntry((ushort)(pointer + lang), entry);
                }
                //Add to temp list
                list[index] = new Dialogue(FlowDataIDX, TextDataIDXs, index);
            }
            Dialogues = new ObservableCollection<Dialogue>(list);
        }
    }
}
