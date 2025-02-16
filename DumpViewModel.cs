using DQB2TextEditor.code;
using Microsoft.Win32;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.TextFormatting;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace DQB2TextEditor
{
    internal class DumpViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private Byte[] linkdataIDXOne;
        private Byte[] linkdataIDXTwo;

        private Byte[] linkdataBinOne;
        private Byte[] linkdataBinTwo;

        private string mLinkdataIDXone;
        private string mLinkdataIDXtwo;

        private VersionInformation dataOne;
        private VersionInformation dataTwo;

        public VersionInformation dataOneShow;
        public VersionInformation dataTwoShow;

        public ObservableCollection<(String, int, int)> Languages { get { return new ObservableCollection<(String, int, int)>(mLanguages); } }
        public int SelectedLanguageIndex { get { return mSelectedLanguageIndex; } set{ mSelectedLanguageIndex = value; 
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedLanguage)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EnabledOne)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EnabledTwo)));
            } }
        public string SelectedLanguage => mLanguages[SelectedLanguageIndex].Item1;

        public bool EmptyExclude { get; set; }
        private int mSelectedLanguageIndex { get; set; }
        private List<(String, int, int)> mLanguages;
        public bool EnabledOne => File.Exists(mLinkdataIDXone) && mSelectedLanguageIndex > -1;
        public bool EnabledTwo => File.Exists(mLinkdataIDXtwo) && mLinkdataIDXtwo != mLinkdataIDXone && mSelectedLanguageIndex > -1;
        public uint LinkdataOneSize => dataOneShow != null ? dataOneShow.LINKDATASize : 0;
        public uint LinkdataTwoSize => dataTwoShow != null ? dataTwoShow.LINKDATASize : 0;

        public string LinkdataIDXone { get { return mLinkdataIDXone; } set { 
                mLinkdataIDXone = value;

                if (File.Exists(mLinkdataIDXone))
                {
                    FileInfo fileInfo = new FileInfo(mLinkdataIDXone);
                    var LinkdataSize = (uint)(fileInfo.Length / 32);

                    dataOne.GetVersionFromSize(LinkdataSize);
                    dataOne.Update();
                    dataOneShow = dataOne;
                }
                else dataOneShow = null;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LinkdataOneSize)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LinkdataOneVersion)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EnabledOne)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LinkdataIDXone)));
                LanguageUpdate();
            } }
        public string LinkdataIDXtwo { get { return mLinkdataIDXtwo; } set { 
                mLinkdataIDXtwo = value;

                if (File.Exists(mLinkdataIDXtwo))
                {
                    FileInfo fileInfo = new FileInfo(mLinkdataIDXtwo);
                    var LinkdataSize = (uint)(fileInfo.Length / 32);

                    dataTwo.GetVersionFromSize(LinkdataSize);
                    dataTwo.Update();
                    dataTwoShow = dataTwo;
                }
                else dataTwoShow = null;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LinkdataTwoSize)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LinkdataTwoVersion)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EnabledTwo)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LinkdataIDXtwo)));
                LanguageUpdate();
            } }

        public string LinkdataOneVersion => dataOneShow != null ? dataOneShow.VersionFile : "---";
        public string LinkdataTwoVersion => dataTwoShow != null ? dataTwoShow.VersionFile : "---";

        
        public DumpViewModel(string linkdataPath, VersionInformation versionInfo)
        {
            dataOne = versionInfo;
            dataTwo = new VersionInformation();
            dataTwo.Update();
            dataOneShow = dataOne;
            dataTwoShow = dataTwo;
            LinkdataIDXone = linkdataPath;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LinkdataOneSize)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LinkdataOneVersion)));
            LanguageUpdate();
        }

        private void LanguageUpdate()
        {
            mLanguages = new();
            if (File.Exists(mLinkdataIDXone) != false)
                foreach (var language in dataOne.Languages)
                {
                    if (File.Exists(mLinkdataIDXtwo) && mLinkdataIDXtwo != mLinkdataIDXone == false)
                        mLanguages.Add((language, dataOne.Languages.IndexOf(language),-1));
                    else if (dataTwo.Languages.Contains(language))
                        mLanguages.Add((language, dataOne.Languages.IndexOf(language), dataTwo.Languages.IndexOf(language)));
                }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Languages)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedLanguage)));
        }
        public void Dump(string name, string type, ProgressChangedEventHandler? UpdateDumpExport, RunWorkerCompletedEventHandler? FinishedDump)
        {
            BackgroundWorker worker = new BackgroundWorker();
            var ListOne = CreateIdxList(LinkdataIDXone, dataOne);
            List<Dialogue> ListTwo = null;
            if (type == "2" || type == "3")
            {
                ListTwo = CreateIdxList(LinkdataIDXtwo, dataTwo);
            }
            worker.WorkerReportsProgress = true;
            switch (type)
            {
                case "0":
                    worker.DoWork += DumpExport;
                    break;
                case "1":
                    worker.DoWork += DumpExportText;
                    break;
                case "2":
                    worker.DoWork += DumpExportCompare;
                    break;
                case "3":
                    worker.DoWork += DumpExportCompareText;
                    break;
            }
            worker.ProgressChanged += UpdateDumpExport;
            worker.RunWorkerCompleted += FinishedDump;

            System.IO.File.Delete(name);
            Object[] dialogues = { ListOne, ListTwo, name };
            worker.RunWorkerAsync(dialogues);
        }
        private async void DumpExport(object sender, DoWorkEventArgs e)
        {
            string line;
            var FullDialogueList = (e.Argument as Object[])[0] as List<Dialogue>;
            var name = (e.Argument as Object[])[2] as string;

            var LinkdataBin = File.ReadAllBytes(mLinkdataIDXone.Substring(0, mLinkdataIDXone.Length - 3) + "BIN");

            var offsetLanguage = mLanguages[SelectedLanguageIndex].Item2;

            foreach (Dialogue Cutscene in FullDialogueList)
            {
                var flowdatabytes = ExtractIDXSRC(Cutscene.CutsceneFile, LinkdataBin); 
                var textidx = ExtractIDXSRC(Cutscene.TextFiles[offsetLanguage], LinkdataBin);
                var textfile = UnpackIDXSRC(textidx);

                line = TextUnpack.TextDump(flowdatabytes, textfile);
                System.IO.File.AppendAllText(name, "============= " + Cutscene.DialogueIndex + " =============\n" + line + "\n", Encoding.UTF8);
                (sender as BackgroundWorker).ReportProgress((int)Cutscene.DialogueIndex, (int)Cutscene.DialogueIndex);
            }
        }
        private async void DumpExportCompare(object sender, DoWorkEventArgs e)
        {
            string line;
            var FullDialogueListOne = (e.Argument as Object[])[0] as List<Dialogue>;
            var FullDialogueListTwo = (e.Argument as Object[])[1] as List<Dialogue>;
            var name = (e.Argument as Object[])[2] as string;

            var LinkdataBinOne = File.ReadAllBytes(mLinkdataIDXone.Substring(0, mLinkdataIDXone.Length - 3) + "BIN");
            var LinkdataBinTwo = File.ReadAllBytes(mLinkdataIDXtwo.Substring(0, mLinkdataIDXtwo.Length - 3) + "BIN");

            var offsetLanguageOne = mLanguages[SelectedLanguageIndex].Item2;
            var offsetLanguageTwo = mLanguages[SelectedLanguageIndex].Item3;

            for (int i = 0; i < 7900; i++)
            {
                var CutsceneOne = FullDialogueListOne[i];
                var CutsceneTwo = FullDialogueListTwo[i];

                if (EmptyExclude && (CutsceneOne.CutsceneFile.CompressedSize == 0 || CutsceneTwo.CutsceneFile.CompressedSize == 0)) continue;

                var flowdatabytesOne = ExtractIDXSRC(CutsceneOne.CutsceneFile, LinkdataBinOne);
                var flowdatabytesTwo = ExtractIDXSRC(CutsceneTwo.CutsceneFile, LinkdataBinTwo);

                var textidxone = ExtractIDXSRC(CutsceneOne.TextFiles[offsetLanguageOne], LinkdataBinOne);
                var textidxtwo = ExtractIDXSRC(CutsceneTwo.TextFiles[offsetLanguageTwo], LinkdataBinTwo);

                var textfileone = UnpackIDXSRC(textidxone);
                var textfiletwo = UnpackIDXSRC(textidxtwo);

                if ((!dataOne.versionEncrypted && !dataTwo.versionEncrypted && !AreEqual(flowdatabytesOne, flowdatabytesTwo)) || (!AreEqual(textfileone, textfiletwo)))
                {
                    line = TextUnpack.TextDump(flowdatabytesOne, textfileone);
                    var lineTwo = TextUnpack.TextDump(flowdatabytesTwo, textfiletwo);
                    if (EmptyExclude && (lineTwo.Length < 2 || line.Length < 2)) continue;
                    System.IO.File.AppendAllText(name, "============= " + CutsceneOne.DialogueIndex + " " + dataOne.VersionFile + " =============\n" + line + "\n", Encoding.UTF8);
                    System.IO.File.AppendAllText(name, "-------- " + CutsceneTwo.DialogueIndex + " " + dataTwo.VersionFile + " --------\n" + lineTwo + "\n", Encoding.UTF8);
                }

                (sender as BackgroundWorker).ReportProgress((int)CutsceneOne.DialogueIndex, (int)CutsceneOne.DialogueIndex);
            }
        }

        private async void DumpExportCompareText(object sender, DoWorkEventArgs e)
        {
            string line;
            var FullDialogueListOne = (e.Argument as Object[])[0] as List<Dialogue>;
            var FullDialogueListTwo = (e.Argument as Object[])[1] as List<Dialogue>;
            var name = (e.Argument as Object[])[2] as string;

            var LinkdataBinOne = File.ReadAllBytes(mLinkdataIDXone.Substring(0, mLinkdataIDXone.Length - 3) + "BIN");
            var LinkdataBinTwo = File.ReadAllBytes(mLinkdataIDXtwo.Substring(0, mLinkdataIDXtwo.Length - 3) + "BIN");

            var offsetLanguageOne = mLanguages[SelectedLanguageIndex].Item2;
            var offsetLanguageTwo = mLanguages[SelectedLanguageIndex].Item3;

            for (int i = 0; i < 7900; i++)
            {
                var CutsceneOne = FullDialogueListOne[i];
                var CutsceneTwo = FullDialogueListTwo[i];

                if (EmptyExclude && (CutsceneOne.CutsceneFile.CompressedSize == 0 || CutsceneTwo.CutsceneFile.CompressedSize == 0)) continue;

                var flowdatabytesOne = ExtractIDXSRC(CutsceneOne.CutsceneFile, LinkdataBinOne);
                var flowdatabytesTwo = ExtractIDXSRC(CutsceneTwo.CutsceneFile, LinkdataBinTwo);

                var textidxone = ExtractIDXSRC(CutsceneOne.TextFiles[offsetLanguageOne], LinkdataBinOne);
                var textidxtwo = ExtractIDXSRC(CutsceneTwo.TextFiles[offsetLanguageTwo], LinkdataBinTwo);

                var textfileone = UnpackIDXSRC(textidxone);
                var textfiletwo = UnpackIDXSRC(textidxtwo);

                if ((!dataOne.versionEncrypted && !dataTwo.versionEncrypted && !AreEqual(flowdatabytesOne, flowdatabytesTwo)) || (!AreEqual(textfileone, textfiletwo)))
                {
                    line = TextUnpack.TextDump(null, textfileone);
                    var lineTwo = TextUnpack.TextDump(null, textfiletwo);
                    if (EmptyExclude && (lineTwo.Length < 2 || line.Length < 2)) continue;
                    System.IO.File.AppendAllText(name, "============= " + CutsceneOne.DialogueIndex + " " + dataOne.VersionFile + " =============\n" + line + "\n", Encoding.UTF8);
                    System.IO.File.AppendAllText(name, "-------- " + CutsceneTwo.DialogueIndex + " " + dataTwo.VersionFile + " --------\n" + lineTwo + "\n", Encoding.UTF8);
                }

                (sender as BackgroundWorker).ReportProgress((int)CutsceneOne.DialogueIndex, (int)CutsceneOne.DialogueIndex);
            }
        }

        public static bool AreEqual(byte[] array1, byte[] array2)
        {
            if (array1 == null || array2 == null)
            {
                return false;
            }

            if (array1.Length != array2.Length)
            {
                return false;
            }

            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i])
                {
                    return false;
                }
            }

            return true;
        }
        private async void DumpExportText(object sender, DoWorkEventArgs e)
        {
            string line;
            var FullDialogueList = (e.Argument as Object[])[0] as List<Dialogue>;
            var name = (e.Argument as Object[])[2] as string;

            var LinkdataBin = File.ReadAllBytes(mLinkdataIDXone.Substring(0, mLinkdataIDXone.Length - 3) + "BIN");

            var offsetLanguage = mLanguages[SelectedLanguageIndex].Item2;

            foreach (Dialogue Cutscene in FullDialogueList)
            {
                var textidx = ExtractIDXSRC(Cutscene.TextFiles[offsetLanguage], LinkdataBin);
                var textfile = UnpackIDXSRC(textidx);

                line = TextUnpack.TextDump(null, textfile);
                System.IO.File.AppendAllText(name, "============= " + Cutscene.DialogueIndex + " =============\n" + line + "\n", Encoding.UTF8);
                (sender as BackgroundWorker).ReportProgress((int)Cutscene.DialogueIndex, (int)Cutscene.DialogueIndex);
            }
        }






        private List<Dialogue> CreateIdxList(String path, VersionInformation VersionInfo)
        {
            Byte[] buffer = System.IO.File.ReadAllBytes(path);

            List<Dialogue> List = new List<Dialogue>();
            uint cutsceneNumber = 0;
            for (int index = VersionInfo.StartCutscene; index < VersionInfo.EndCutscene; index++)
            {
                var idx = new IDX(index);

                idx.Offset = BitConverter.ToUInt64(buffer, index * 32 + 0);
                idx.UncompressedSize = BitConverter.ToUInt64(buffer, index * 32 + 8);
                idx.CompressedSize = BitConverter.ToUInt64(buffer, index * 32 + 16);
                idx.IsCompressed = BitConverter.ToUInt64(buffer, index * 32 + 24);

                List.Add(new Dialogue(cutsceneNumber, idx));
                cutsceneNumber++;
            }
            cutsceneNumber = 0;
            for (int index = VersionInfo.StartText; index < VersionInfo.EndText; index += VersionInfo.LanguageCount)
            {
                var TextFileList = new List<IDX>();
                for (int relativeIndex = 0; relativeIndex < VersionInfo.LanguageCount; relativeIndex++)
                {
                    var localIndex = index + relativeIndex;
                    var idx = new IDX(localIndex);

                    idx.Offset = BitConverter.ToUInt64(buffer, localIndex * 32 + 0);
                    idx.UncompressedSize = BitConverter.ToUInt64(buffer, localIndex * 32 + 8);
                    idx.CompressedSize = BitConverter.ToUInt64(buffer, localIndex * 32 + 16);
                    idx.IsCompressed = BitConverter.ToUInt64(buffer, localIndex * 32 + 24);

                    TextFileList.Add(idx);
                }
                List.ElementAt((int)cutsceneNumber).TextFiles = TextFileList;
                cutsceneNumber++;
            }
            return List;
        }

        private Byte[] ExtractIDXSRC(IDX Cutscene, Byte[] bin)
        {
            //Cutscene
            Byte[] CutsceneBuffer = new Byte[(uint)Cutscene.UncompressedSize];
            Array.Copy(bin, (int)Cutscene.Offset, CutsceneBuffer, 0, CutsceneBuffer.Length);

            return CutsceneBuffer;
        }

        private Byte[] UnpackIDXSRC(Byte[] src)
        {
            //Text File
            int size = src.Length;

            IDXzrc idxzrc = new IDXzrc();
            idxzrc.ReadInternal(src);

            Byte[] bufferUncompressed = new Byte[idxzrc.UncompressedSize];

            int index = 0;
            foreach (var chunk in idxzrc.Chunks)
            {
                int sizeNew = BitConverter.ToInt32(src, (int)chunk.Offset);
                Byte[] tmp = new Byte[sizeNew];
                Array.Copy(src, chunk.Offset + 4, tmp, 0, tmp.Length);
                tmp = Decomp(tmp);
                Array.Copy(tmp, 0, bufferUncompressed, index, tmp.Length);
                index += tmp.Length;
            }
            return bufferUncompressed;
        }

        private Byte[] Comp(Byte[] data)
        {
            Byte[] result = [];
            using (var input = new MemoryStream(data))
            {
                using (var output = new MemoryStream())
                {
                    using (var zlib = new System.IO.Compression.ZLibStream(output, System.IO.Compression.CompressionLevel.Fastest))
                    {
                        input.CopyTo(zlib);
                    }
                    result = output.ToArray();
                }
            }
            return result;
        }

        private Byte[] Decomp(Byte[] data)
        {
            Byte[] result = [];
            using (var input = new MemoryStream(data))
            {
                using (var zlib = new System.IO.Compression.ZLibStream(input, System.IO.Compression.CompressionMode.Decompress))
                {
                    using (var output = new MemoryStream())
                    {
                        zlib.CopyTo(output);
                        result = output.ToArray();
                    }
                }
            }
            return result;
        }
    }
}
