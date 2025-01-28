using DQB2TextEditor.code;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;


namespace DQB2TextEditor
{
    public class ViewModel
    {
        public ObservableProperty<bool> ConfirmEnabled { get; set; } = new ObservableProperty<bool>() { };
        public ObservableProperty<string> LinkdataPath { get; set; } = new ObservableProperty<string>() { };
        public ObservableProperty<uint> LinkdataEntries { get; set; } = new ObservableProperty<uint>() { };
        public VersionInformation VersionInfo { get; set; } = new VersionInformation();
        public ICommand CommandLinkdataPathUpdate { get; private set; }
        public ICommand CommandLoadDialogue { get; private set; }
        public ICommand CommandSaveDialogue { get; private set; }
        public ICommand CommandExportFlowData { get; private set; }
        public ICommand CommandExportTextData { get; private set; }
        public ICommand CommandExportSelectedFlowData { get; private set; }
        public ICommand CommandExportSelectedTextData { get; private set; }
        public ICommand CommandImportFlowData { get; private set; }
        public ICommand CommandImportTextData { get; private set; }
        public ICommand CommandExportTxt { get; private set; }
        public ICommand CommandExportSelectedTxt { get; private set; }
        public ICommand CommandImportTxt { get; private set; }
        public ICommand CommandExportProcessedTxt { get; private set; }
        public ICommand CommandExportSelectedProcessedTxt { get; private set; }
        public ObservableCollection<Dialogue> FilteredDialogueList { get; set; } = new ObservableCollection<Dialogue>();
        public ObservableCollection<Dialogue> FilteredIndependentTextList { get; set; } = new ObservableCollection<Dialogue>();

        public ObservableProperty<Dialogue> CurrentDialogue { get; set; } = new ObservableProperty<Dialogue>();
        public static ushort CurrentSelectedLanguage { get; set; } = 0;

        public ObservableProperty<TextUnpack> UnpackedText { get; set; } = new ObservableProperty<TextUnpack>();
        public Dialogue UnpackedDialogue;
        private List<Dialogue> FullDialogueList;
        private List<Dialogue> FullIndependentTextList;

        public bool JPPopup
        {
            get => VersionInformation.JapaneseMode; set
            {
                VersionInformation.JapaneseMode = value;
            }
        }
        public ObservableProperty<Visibility> LinkdataVisibility { get; private set; } = new ObservableProperty<Visibility> { Value = Visibility.Visible };
        public ObservableProperty<Visibility> TextHelpVisibility { get; private set; } = new ObservableProperty<Visibility> { Value = Visibility.Collapsed };
        private bool mTextPanel = false;
        private bool mPreviewtext = false;
        private System.Windows.Controls.ListBox ForUpdatingBindings;
        private System.Windows.Controls.ScrollViewer ForUpdatingText;
        private System.Windows.Controls.ListBox ForUpdatingTextList;
        private System.Windows.Controls.Label Saving;
        private CutsceneBrowser window;
        public bool PreviewText
        {
            get
            {
                return mPreviewtext;
            }
            set
            {
                mPreviewtext = value;
                if (UnpackedDialogue != null)
                    UnpackedText.Value.SwapPreview(value);
            }
        }
        public bool TextPanel
        {
            get
            {
                return mTextPanel;
            }
            set
            {
                mTextPanel = value;
                if (value)
                {
                    LinkdataVisibility.Value = Visibility.Collapsed;
                    TextHelpVisibility.Value = Visibility.Visible;
                }
                else
                {
                    LinkdataVisibility.Value = Visibility.Visible;
                    TextHelpVisibility.Value = Visibility.Collapsed;
                }
            }
        }
        public ViewModel()
        {
            CommandLinkdataPathUpdate = new CommandAction(LinkdataPathUpdate);
            CommandLoadDialogue = new CommandAction(LoadDialogue);
            CommandSaveDialogue = new CommandAction(SaveDialogue);

            CommandImportTxt = new CommandAction(ImportTxt);
            CommandImportFlowData = new CommandAction(ImportFlowData);
            CommandImportTextData = new CommandAction(ImportTextData);

            CommandExportTxt = new CommandAction(ExportTxt);
            CommandExportFlowData = new CommandAction(ExportFlowData);
            CommandExportTextData = new CommandAction(ExportTextData);
            CommandExportProcessedTxt = new CommandAction(ExportProcessedTxt);

            CommandExportSelectedTxt = new CommandAction(ExportSelectedTxt);
            CommandExportSelectedFlowData = new CommandAction(ExportSelectedFlowData);
            CommandExportSelectedTextData = new CommandAction(ExportSelectedTextData);
            CommandExportSelectedProcessedTxt = new CommandAction(ExportSelectedProcessedTxt);
        }
        public void ContextChangeOne()
        {
            CreateIdxList();
        }
        public void ContextChangeTwo(System.Windows.Controls.ListBox listBox, System.Windows.Controls.ScrollViewer textBox, System.Windows.Controls.ListBox rawListBox, System.Windows.Controls.Label Saving, CutsceneBrowser JustHaveItWhatever)
        {
            ForUpdatingBindings = listBox;
            ForUpdatingText = textBox;
            ForUpdatingTextList = rawListBox;
            this.Saving = Saving;
            window = JustHaveItWhatever;
        }
        private void ExportSelectedTxt(object param)
        {
            String Filename = "DialogueText" + CurrentDialogue.Value.DialogueIndex.ToString("D5");
            if (CurrentDialogue.Value.CutsceneFile == null)
                Filename = "Text" + CurrentDialogue.Value.DialogueIndex.ToString("D5");

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "*.txt|*.txt",
                FileName = Filename
            };
            if (saveFileDialog.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }
            Saving.Visibility = Visibility.Visible;
            window.DisableOnLoad.IsEnabled = false;
            Saving.UpdateLayout();
            window.UpdateLayout();

            TextUnpack TextUnpackVar = null;
            if (CurrentDialogue.Value.CutsceneFile == null)
            {
                var ByteData = UnpackIDXFiles(CurrentDialogue.Value.CurrentTextFile);
                TextUnpackVar = new TextUnpack(ByteData, mPreviewtext);
            }
            else
            {
                var bytedata = UnpackIDXFiles(CurrentDialogue.Value.CutsceneFile, CurrentDialogue.Value.CurrentTextFile);
                TextUnpackVar = new TextUnpack(bytedata.Item1, bytedata.Item2);
            }
            TextUnpackVar.SaveTxtFile(saveFileDialog.FileName, false);
            Saving.Visibility = Visibility.Hidden;
            window.DisableOnLoad.IsEnabled = true;
        }
        private void ExportSelectedFlowData(object param)
        {
            if (CurrentDialogue.Value.CutsceneFile == null) return;

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "*.unpack|*.unpack",
                FileName = "FlowData" + CurrentDialogue.Value.DialogueIndex.ToString("D5")
            };

            if (saveFileDialog.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }
            System.IO.File.WriteAllBytes(saveFileDialog.FileName, UnpackIDXFiles(CurrentDialogue.Value.CutsceneFile, CurrentDialogue.Value.CurrentTextFile).Item1);
        }
        private void ExportSelectedTextData(object param)
        {
            String Filename = "DialogueTextData" + CurrentDialogue.Value.DialogueIndex.ToString("D5");
            if (CurrentDialogue.Value.CutsceneFile == null)
                Filename = "TextData" + CurrentDialogue.Value.DialogueIndex.ToString("D5");

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "*.unpack|*.unpack",
                FileName = Filename
            };
            if (saveFileDialog.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }
            System.IO.File.WriteAllBytes(saveFileDialog.FileName, UnpackIDXFiles(CurrentDialogue.Value.CurrentTextFile));

        }
        private void ExportSelectedProcessedTxt(object param)
        {
            String Filename = "ProcessedDialogueText" + CurrentDialogue.Value.DialogueIndex.ToString("D5");
            if (CurrentDialogue.Value.CutsceneFile == null)
                Filename = "DialogueText" + CurrentDialogue.Value.DialogueIndex.ToString("D5");

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "*.txt|*.txt",
                FileName = Filename
            };
            if (saveFileDialog.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }
            Saving.Visibility = Visibility.Visible;
            window.DisableOnLoad.IsEnabled = false;
            Saving.UpdateLayout();
            window.UpdateLayout();
            TextUnpack TextUnpackVar = null;
            if (CurrentDialogue.Value.CutsceneFile == null)
            {
                var ByteData = UnpackIDXFiles(CurrentDialogue.Value.CurrentTextFile);
                TextUnpackVar = new TextUnpack(ByteData, mPreviewtext);
            }
            else
            {
                var bytedata = UnpackIDXFiles(CurrentDialogue.Value.CutsceneFile, CurrentDialogue.Value.CurrentTextFile);
                TextUnpackVar = new TextUnpack(bytedata.Item1, bytedata.Item2);
            }
            TextUnpackVar.SaveTxtFile(saveFileDialog.FileName, true);
            Saving.Visibility = Visibility.Hidden;
            window.DisableOnLoad.IsEnabled = true;
        }

        private void ExportFlowData(object param)
        {
            if (UnpackedDialogue == null) return;
            if (!UnpackedText.Value.IsCutscene) return;
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "*.unpack|*.unpack",
                FileName = "FlowData" + UnpackedDialogue.DialogueIndex.ToString("D5")
            };
            if (saveFileDialog.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }
            UnpackedText.Value.SaveFlowData(saveFileDialog.FileName);
        }
        private void ImportFlowData(object param)
        {
            if (UnpackedDialogue == null) return;
            if (!UnpackedText.Value.IsCutscene) return;

            var openFileDialog = new OpenFileDialog
            {
                Filter = "*.unpack|*.unpack"
            };

            if (openFileDialog.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }
            FileInfo fileInfo = new FileInfo(openFileDialog.FileName);
            if (fileInfo.Length % 0x34 != 0) return;

            UnpackedText.Value.UpdateFlowData(System.IO.File.ReadAllBytes(openFileDialog.FileName));
            UnpackedText.NotifyValue();
        }
        private void ExportTextData(object param)
        {
            if (UnpackedDialogue == null) return;
            String Filename = "DialogueTextData" + UnpackedDialogue.DialogueIndex.ToString("D5");
            if (!UnpackedText.Value.IsCutscene)
                Filename = "TextData" + UnpackedDialogue.DialogueIndex.ToString("D5");
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "*.unpack|*.unpack",
                FileName = Filename
            };
            if (saveFileDialog.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }
            UnpackedText.Value.SaveTextData(saveFileDialog.FileName);
        }
        private void ImportTextData(object param)
        {
            if (UnpackedDialogue == null) return;

            var openFileDialog = new OpenFileDialog
            {
                Filter = "*.unpack|*.unpack"
            };

            if (openFileDialog.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }

            UnpackedText.Value.UpdateTxtData(System.IO.File.ReadAllBytes(openFileDialog.FileName));
            UnpackedText.NotifyValue();
        }
        private void ExportTxt(object param)
        {
            if (UnpackedDialogue == null) return;
            String Filename = "DialogueText" + UnpackedDialogue.DialogueIndex.ToString("D5");
            if (!UnpackedText.Value.IsCutscene)
                Filename = "Text" + UnpackedDialogue.DialogueIndex.ToString("D5");
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "*.txt|*.txt",
                FileName = Filename
            };
            if (saveFileDialog.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }

            Saving.Visibility = Visibility.Visible;
            window.DisableOnLoad.IsEnabled = false;
            Saving.UpdateLayout();
            window.UpdateLayout();

            UnpackedText.Value.SaveTxtFile(saveFileDialog.FileName, false);
            Saving.Visibility = Visibility.Hidden;
            window.DisableOnLoad.IsEnabled = true;
        }

        private void ExportProcessedTxt(object param)
        {
            if (UnpackedDialogue == null) return;
            String Filename = "ProcessedDialogueText" + UnpackedDialogue.DialogueIndex.ToString("D5");
            if (!UnpackedText.Value.IsCutscene)
                Filename = "ProcessedText" + UnpackedDialogue.DialogueIndex.ToString("D5");
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "*.txt|*.txt",
                FileName = Filename
            };
            if (saveFileDialog.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }

            Saving.Visibility = Visibility.Visible;
            window.DisableOnLoad.IsEnabled = false;
            Saving.UpdateLayout();
            window.UpdateLayout();

            UnpackedText.Value.SaveTxtFile(saveFileDialog.FileName, true);
            Saving.Visibility = Visibility.Hidden;
            window.DisableOnLoad.IsEnabled = true;
        }
        private void ImportTxt(object param)
        {
            if (UnpackedDialogue == null) return;

            var openFileDialog = new OpenFileDialog
            {
                Filter = "*.txt|*.TXT"
            };
            if (openFileDialog.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }

            Saving.Visibility = Visibility.Visible;
            window.DisableOnLoad.IsEnabled = false;
            Saving.UpdateLayout();
            window.UpdateLayout();

            UnpackedText.Value.UpdateTextString(openFileDialog.FileName);
            UnpackedText.NotifyValue();
            Saving.Visibility = Visibility.Hidden;
            window.DisableOnLoad.IsEnabled = true;
        }
        private void LinkdataPathUpdate(object param)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "LINKDATA|LINKDATA.IDX";
            if (dlg.ShowDialog() == false) return;

            FileInfo fileInfo = new FileInfo(dlg.FileName);
            LinkdataEntries.Value = (uint)(fileInfo.Length / 32);

            LinkdataPath.Value = dlg.FileName;
            LinkdataPath.NotifyValue();
            if (VersionInfo.LINKDATASize != LinkdataEntries.Value) ConfirmEnabled.Value = false;
            else ConfirmEnabled.Value = true;
            ConfirmEnabled.NotifyValue();
        }
        private void mLoadDialogue()
        {
            if (CurrentDialogue.Value.CutsceneFile == null)
            {
                var ByteData = UnpackIDXFiles(CurrentDialogue.Value.CurrentTextFile);
                UnpackedDialogue = CurrentDialogue.Value;
                if (UnpackedText.Value == null)
                    UnpackedText.Value = new TextUnpack(ByteData, mPreviewtext);
                else
                    UnpackedText.Value = new TextUnpack(ByteData, UnpackedText.Value.RawTextAttempt, mPreviewtext);
                ForUpdatingBindings.Visibility = Visibility.Collapsed;
                if (UnpackedText.Value.RawTextShow)
                {
                    ForUpdatingText.Visibility = Visibility.Visible;
                    ForUpdatingTextList.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ForUpdatingText.Visibility = Visibility.Collapsed;
                    ForUpdatingTextList.Visibility = Visibility.Visible;
                }
            }
            else
            {
                var ByteData = UnpackIDXFiles(CurrentDialogue.Value.CutsceneFile, CurrentDialogue.Value.CurrentTextFile);
                UnpackedDialogue = CurrentDialogue.Value;
                if (UnpackedText.Value == null)
                    UnpackedText.Value = new TextUnpack(ByteData.Item1, ByteData.Item2);
                else
                    UnpackedText.Value = new TextUnpack(ByteData.Item1, ByteData.Item2, UnpackedText.Value.Text, mPreviewtext);
                ForUpdatingText.Visibility = Visibility.Collapsed;
                ForUpdatingBindings.Visibility = Visibility.Visible;
                ForUpdatingTextList.Visibility = Visibility.Collapsed;

            }
        }
        private void LoadDialogue(object param)
        {
            Saving.Visibility = Visibility.Visible;
            window.DisableOnLoad.IsEnabled = false;
            mLoadDialogue(); //whatever ;-;
            Saving.Visibility = Visibility.Hidden;
            window.DisableOnLoad.IsEnabled = true;
        }
        private void SaveDialogue(object param)
        {
            if (CurrentDialogue.Value.CutsceneFile == null && UnpackedText.Value.IsCutscene) return;
            if (CurrentDialogue.Value.CutsceneFile != null && !UnpackedText.Value.IsCutscene) return;
            Saving.Visibility = Visibility.Visible;
            window.DisableOnLoad.IsEnabled = false;
            Saving.UpdateLayout();
            window.UpdateLayout();
            UnpackedText.Value.EntryToBytes();
            if (CurrentDialogue.Value.CutsceneFile != null)
            {
                ImportIDX(CurrentDialogue.Value.CutsceneFile, UnpackedText.Value.GetCutsceneFile);
            }
            ImportIDX(CurrentDialogue.Value.CurrentTextFile, PackIDXzrc(UnpackedText.Value.GetTextFile));
            CreateIdxList();
            Saving.Visibility = Visibility.Hidden;
            window.DisableOnLoad.IsEnabled = true;
        }
        public void UpdateSelectedDialogue(Dialogue New)
        {
            CurrentDialogue.Value = New;
            CurrentDialogue.NotifyValue();
        }
        public void FilterList(string filter)
        {
            if (FullDialogueList == null) return;
            if (filter == null)
            {
                FilteredDialogueList = new ObservableCollection<Dialogue>(FullDialogueList);
                FilteredIndependentTextList = new ObservableCollection<Dialogue>(FullIndependentTextList);
                return;
            }
            FilteredDialogueList = new ObservableCollection<Dialogue>();
            foreach (var a in FullDialogueList)
            {
                if (a.Preview.Contains(filter) || a.DialogueIndex.ToString().Contains(filter) || a.CutsceneIndex.ToString().Contains(filter) || a.TextIndex.ToString().Contains(filter))
                {
                    FilteredDialogueList.Add(a);
                }
            }
            FilteredIndependentTextList = new ObservableCollection<Dialogue>();
            foreach (var a in FullIndependentTextList)
            {
                if (a.Preview.Contains(filter) || a.DialogueIndex.ToString().Contains(filter) || a.TextIndex.ToString().Contains(filter))
                {
                    FilteredIndependentTextList.Add(a);
                }
            }
        }

        /* NEXT CODE COMES FROM TURTLE-INSECT:

         https://github.com/turtle-insect/DQB2

         -> CreateIdxList()
         -> UnpackIDXFiles()
         -> ImportIDX()
         -> PackIDXzrc()
         -> Comp()
         -> Decomp()
        */

        private void CreateIdxList()
        {
            String path = LinkdataPath.Value;
            if (!System.IO.File.Exists(path)) return;

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
            FullDialogueList = List;
            //IndeoendentText
            cutsceneNumber = 0;
            List = new List<Dialogue>();
            foreach (var Range in VersionInfo.IndividualText)
                for (int index = Range.Item1; index < Range.Item1 + Range.Item2; index += VersionInfo.LanguageCount)
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
                    List.Add(new Dialogue(cutsceneNumber, TextFileList));
                    cutsceneNumber++;
                }
            FullIndependentTextList = List;
            FilterList(null);
        }
        private (Byte[], Byte[]) UnpackIDXFiles(IDX Cutscene, IDX idx)
        {
            String path = LinkdataPath.Value;
            if (!System.IO.File.Exists(path)) return (null, null);
            path = path.Substring(0, path.Length - 3) + "BIN";
            if (!System.IO.File.Exists(path)) return (null, null);

            if (Cutscene == null || idx == null) return (null, null);

            Byte[] bin = System.IO.File.ReadAllBytes(path);

            //Cutscene
            Byte[] CutsceneBuffer = new Byte[(uint)Cutscene.UncompressedSize];
            Array.Copy(bin, (int)Cutscene.Offset, CutsceneBuffer, 0, CutsceneBuffer.Length);

            //Text File
            int size = (int)idx.UncompressedSize;
            Byte[] buffer = new Byte[size];
            Array.Copy(bin, (int)idx.Offset, buffer, 0, buffer.Length);
            IDXzrc idxzrc = new IDXzrc();
            idxzrc.ReadInternal(buffer);

            Byte[] bufferUncompressed = new Byte[idxzrc.UncompressedSize];

            int index = 0;
            foreach (var chunk in idxzrc.Chunks)
            {
                int sizeNew = BitConverter.ToInt32(buffer, (int)chunk.Offset);
                Byte[] tmp = new Byte[sizeNew];
                Array.Copy(buffer, chunk.Offset + 4, tmp, 0, tmp.Length);
                tmp = Decomp(tmp);
                Array.Copy(tmp, 0, bufferUncompressed, index, tmp.Length);
                index += tmp.Length;
            }
            return (CutsceneBuffer, bufferUncompressed);
        }
        private Byte[] UnpackIDXFiles(IDX idx)
        {
            String path = LinkdataPath.Value;
            if (!System.IO.File.Exists(path)) return null;
            path = path.Substring(0, path.Length - 3) + "BIN";
            if (!System.IO.File.Exists(path)) return null;

            Byte[] bin = System.IO.File.ReadAllBytes(path);

            //Text File
            int size = (int)idx.UncompressedSize;
            Byte[] buffer = new Byte[size];
            Array.Copy(bin, (int)idx.Offset, buffer, 0, buffer.Length);
            IDXzrc idxzrc = new IDXzrc();
            idxzrc.ReadInternal(buffer);

            Byte[] bufferUncompressed = new Byte[idxzrc.UncompressedSize];

            int index = 0;
            foreach (var chunk in idxzrc.Chunks)
            {
                int sizeNew = BitConverter.ToInt32(buffer, (int)chunk.Offset);
                Byte[] tmp = new Byte[sizeNew];
                Array.Copy(buffer, chunk.Offset + 4, tmp, 0, tmp.Length);
                tmp = Decomp(tmp);
                Array.Copy(tmp, 0, bufferUncompressed, index, tmp.Length);
                index += tmp.Length;
            }
            return bufferUncompressed;
        }

        private void ImportIDX(IDX idx, byte[] importFile)
        {
            String path = LinkdataPath.Value;
            if (!System.IO.File.Exists(path)) return;
            path = path.Substring(0, path.Length - 3) + "BIN";
            if (!System.IO.File.Exists(path)) return;

            uint size = 0;
            if (idx.IsCompressed == 0)
            {
                size = (uint)importFile.Length;
            }
            else
            {
                IDXzrc idxzrc = new IDXzrc();
                idxzrc.ReadInternal(importFile);
                size = idxzrc.UncompressedSize;
            }

            Byte[] link_idx = System.IO.File.ReadAllBytes(LinkdataPath.Value);
            Byte[] link_bin = System.IO.File.ReadAllBytes(path);

            // offset = after - original
            // original file
            int offset = ((int)idx.CompressedSize + 0x80) / 0x100 * 0x100;
            // after file
            offset = (importFile.Length + 0x80) / 0x100 * 0x100 - offset;


            Byte[] bin = new Byte[link_bin.Length + offset];
            Array.Copy(link_bin, 0, bin, 0, (int)idx.Offset);

            Array.Fill<Byte>(link_idx, 0, idx.Index * 32 + 8, 24);
            Array.Copy(BitConverter.GetBytes(size), 0, link_idx, idx.Index * 32 + 8, 4);
            Array.Copy(BitConverter.GetBytes(importFile.Length), 0, link_idx, idx.Index * 32 + 16, 4);
            Array.Copy(BitConverter.GetBytes(idx.IsCompressed), 0, link_idx, idx.Index * 32 + 24, 4);
            Array.Copy(importFile, 0, bin, (int)idx.Offset, importFile.Length);

            for (int index = idx.Index + 1; index < link_idx.Length / 32; index++)
            {
                var IDXofset = BitConverter.ToUInt64(link_idx, index * 32 + 0);
                var IDXcompressed = BitConverter.ToUInt64(link_idx, index * 32 + 16);
                UInt64 address = IDXofset + (UInt64)offset;
                Array.Copy(BitConverter.GetBytes((UInt64)address), 0, link_idx, index * 32, 8);
                Array.Copy(link_bin, (int)IDXofset, bin, (int)address, (int)IDXcompressed);
            }

            System.IO.File.WriteAllBytes(LinkdataPath.Value, link_idx);
            System.IO.File.WriteAllBytes(path, bin);
        }
        public Int32 PackSplitSize { get; set; } = 0x200000;
        private byte[] PackIDXzrc(byte[] importFile)
        {
            Int32 packCount = (importFile.Length + PackSplitSize - 1) / PackSplitSize;

            using var ms = new MemoryStream();
            // split size.
            ms.Write(BitConverter.GetBytes(PackSplitSize), 0, 4);
            // split count.
            ms.Write(BitConverter.GetBytes(packCount), 0, 4);
            // unpack file size.
            ms.Write(BitConverter.GetBytes(importFile.Length), 0, 4);

            // chunk size.
            // reserve.
            // overwrite when writing data
            for (int index = 0; index < packCount * 4; index++)
            {
                ms.WriteByte(0);
            }

            // padding.
            int count = 0x80 - ((int)ms.Length % 0x80);
            if (count == 0x80) count = 0;
            for (int index = 0; index < count; index++)
            {
                ms.WriteByte(0);
            }

            // data
            for (int pack = 0; pack < packCount; pack++)
            {
                int length = PackSplitSize;
                if (pack + 1 == packCount) length = importFile.Length % PackSplitSize;
                Byte[] tmp = new Byte[length];
                Array.Copy(importFile, PackSplitSize * pack, tmp, 0, tmp.Length);
                tmp = Comp(tmp);

                // rewrite chunk size
                ms.Seek(0x0C + pack * 4, SeekOrigin.Begin);
                ms.Write(BitConverter.GetBytes(tmp.Length + 4), 0, 4);
                // chunk data
                // size & bytes
                ms.Seek(0, SeekOrigin.End);
                ms.Write(BitConverter.GetBytes(tmp.Length), 0, 4);
                ms.Write(tmp, 0, tmp.Length);

                // padding.
                count = 0x80 - ((int)ms.Length % 0x80);
                if (count == 0x80) count = 0;
                for (int index = 0; index < count; index++)
                {
                    ms.WriteByte(0);
                }
            }
            return ms.ToArray();
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
