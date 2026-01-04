using DQB2TextEditor.InfoReading;
using DQB2TextEditor.Linkdata;
using DQB2TextEditor.Linkdata.LineEntry;
using DQB2TextEditor.Proccessing;
using DQB2TextEditor.Windows.Panel;
using DQB2TextEditor.Windows.UserControlFolder;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

using System.Windows.Media;


namespace DQB2TextEditor.Windows
{
    internal class ViewModel : ObtainData, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public static ViewModel me;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static LINKDATA linkdata { get; private set; }

        public bool Asia => linkdata.AsianLanguages.Contains(_currentLanguage); //Format of the text preview.


        public byte CurrentLinkdataFile
        {
            get { return _currentLinkdataFile; }
            set
            {
                if (_currentLinkdataFile != value)
                {
                    _currentLinkdataFile = value;
                }
            }
        }
        public byte CurrentLanguage
        {
            get { return _currentLanguage; }
            set
            {
                if (_currentLanguage != value)
                {
                    _currentLanguage = value;
                    OnPropertyChanged(nameof(PNameDef));
                    OnPropertyChanged(nameof(CurrentLanguage));
                    OnPropertyChanged(nameof(SelectedTextGroup));
                    OnPropertyChanged(nameof(_currentLanguage));
                    OnPropertyChanged(nameof(PreviewFontFamily));
                }
            }
        }

        public static byte _currentLinkdataFile = 0;
        public static byte _currentLanguage = 0;
        public bool selected => _selectedTextGroup != null;
        public Visibility DialogueLoaded => _selectedTextGroup is Dialogue ? Visibility.Visible : Visibility.Collapsed;


        public ObservableCollection<Dialogue> Dialogues { get; private set; }
        public ObservableCollection<TextGroup> MenuTexts { get; private set; }
        public ObservableCollection<String> TextLinesPreview { get; private set; } = new ObservableCollection<string>();

        //For text editing.
        public ObservableCollection<String> TextLinesEdit { get; private set; } = new ObservableCollection<string>();

        public ObservableCollection<FlowDataLine> DialogueLinesEdit { get; private set; } = new ObservableCollection<FlowDataLine>();

        private TextGroup _selectedTextGroup;

        private TextEditorWindow window;
      
        public int PreviewIndex { get; private set; }
        private bool _dialogue;
        public int DialogueHeight => _dialogue ? 63 : 0;
        public double EntryWidth => window.listBox.ActualWidth - 26;
        public bool editing => _editingTextGroup != null;

        public Visibility selectedVisibility => _selectedTextGroup != null ? Visibility.Visible : Visibility.Collapsed;
        public Visibility editingVisibility => _editingTextGroup != null ? Visibility.Visible : Visibility.Collapsed;

        private TextGroup _editingTextGroup;

        public int TextWidth => window == null ? 0 : (int)window.BorderPreview.ActualWidth - 34;
        public TextGroup SelectedTextGroup
        {
            get { return _selectedTextGroup; }
            set
            {
                if (_selectedTextGroup != value)
                {
                    _selectedTextGroup = value;
                    OnPropertyChanged(nameof(SelectedTextGroup));
                    OnPropertyChanged(nameof(selectedVisibility));
                    OnPropertyChanged(nameof(Selection));
                    OnPropertyChanged(nameof(selected));
                }
            }
        }
        public TextGroup EditingTextGroup
        {
            get { return _editingTextGroup; }
            set
            {
                if (_editingTextGroup != value)
                {
                    _editingTextGroup = value;
                    OnPropertyChanged(nameof(EditingTextGroup));
                    OnPropertyChanged(nameof(EditedSelection));
                    OnPropertyChanged(nameof(editingVisibility));
                    OnPropertyChanged(nameof(editing));

                    OnPropertyChanged(nameof(IsDialogue));
                    OnPropertyChanged(nameof(IsText));

                    OnPropertyChanged(nameof(PreviewFontSize));
                    OnPropertyChanged(nameof(PreviewLineSpace));
                    OnPropertyChanged(nameof(PreviewPadding));
                    OnPropertyChanged(nameof(PreviewFontFamily));
                    OnPropertyChanged(nameof(PreviewFontSizeFurigana));
                    EditingLine = "";
                }
            }
        }
        public string Selection => SelectedTextGroup is Dialogue ? "Dialogue" : "Text";
        public string EditedSelection => EditingTextGroup is Dialogue ? "Dialogue" : "Text";

        public Visibility IsDialogue => EditingTextGroup is Dialogue && (!linkdata.Encrypted) ? Visibility.Visible : Visibility.Collapsed;
        public Visibility IsText => linkdata.Encrypted || !(EditingTextGroup is Dialogue) ? Visibility.Visible : Visibility.Collapsed;

        private FontFamily PreviewFontFamilyEU = new FontFamily(new Uri("pack://application:,,,/"),"./Info/#DQB2_2");
        private FontFamily PreviewFontFamilyAS = SystemFonts.MessageFontFamily;
        public FontFamily PreviewFontFamily => Asia ? PreviewFontFamilyAS : PreviewFontFamilyEU;
        public int PreviewFontSize => PreviewFontFamily == PreviewFontFamilyEU ? 14 : 12;
        public int PreviewLineSpace => PreviewFontFamily == PreviewFontFamilyEU ? 13 : 18;
        public Thickness PreviewPadding => PreviewFontFamily == PreviewFontFamilyEU ? new Thickness(12, 7,12,7) : new Thickness(12, 2, 12, 2);
        public int PreviewFontSizeFurigana => 5;

        private String playerName;
        private bool gender = false;
        public String PName { get => playerName; set { playerName = value; 
                OnPropertyChanged(nameof(PName));
                OnPropertyChanged(nameof(NameValid));
            } }
        public String PNameDef => gender ? (Asia ? "クリエ" : "Creatrix") : (Asia ? "ビルド" : "Bildrick");

        public Visibility NameValid => String.IsNullOrEmpty(playerName) ? Visibility.Visible : Visibility.Collapsed;

        public static string PlayerName => String.IsNullOrEmpty(me.PName) ? me.PNameDef : me.PName;
        public static bool Gender => me.gender;
        public bool Male
        {
            get => !gender;
            set
            {
                gender = !value;
                OnPropertyChanged(nameof(PNameDef));
                OnPropertyChanged(nameof(Male));
                OnPropertyChanged(nameof(Female));
            }
        }
        public bool Female
        {
            get => gender;
            set
            {
                gender = value;
                OnPropertyChanged(nameof(PNameDef));
                OnPropertyChanged(nameof(Male));
                OnPropertyChanged(nameof(Female));
            }
        }

        public String FilterText { get; set; } = String.Empty;
        public String FilterTextGeneral { get; set; } = String.Empty;

        public ViewModel(SLViewModel SLVM, TextEditorWindow window)
        {
            me = this;
            this.window = window;
            linkdata = SLVM.CreateLINKDATA();
            Dialogues = linkdata.Dialogues;
            MenuTexts = linkdata.MenuTexts;
            InformationReading.vModel = this;
        }

        public void UpdatePreviewText()
        {
            TextLinesPreview.Clear();
            var mad = SelectedTextGroup.GetTextLinesPreview().ToList();
            foreach(var line in mad){
                if (!String.IsNullOrEmpty(line) && !line.Equals("\0"))
                    TextLinesPreview.Add(line);
            }
            PreviewIndex = SelectedTextGroup.TextDataIndex;
            OnPropertyChanged(nameof(PreviewIndex));
            _dialogue = SelectedTextGroup is Dialogue;
            OnPropertyChanged(nameof(DialogueHeight));
        }

        public async void TextFilterGeneral()
        {
            var newTexts = new ObservableCollection<TextGroup>();
            var progressWindow = new ProgressWindow("Searching for string...", "Note: Things like names, player pronouns or other generated text wont filter properly."+Environment.NewLine +"Note 2: Text files are a bit broken so the search will sometimes hang on a few files.", (uint)linkdata.MenuTexts.Count);
            progressWindow.Show();
            var filterText = FilterTextGeneral?.Trim().ToLower() ?? String.Empty;

            window.IsHitTestVisible = false;
            await Task.Run(() =>
            {
                int i = 0;
                foreach (var textGroup in linkdata.MenuTexts)
                {
                    var mad = textGroup.GetTextLinesPreview().ToList();
                    foreach (var line in mad)
                    {
                        var newline = line?.Trim().ToLower() ?? String.Empty;
                        if (!String.IsNullOrEmpty(newline) && !newline.Equals("\0"))
                        {
                            if (newline.Contains(filterText) || Regex.Replace(newline, @"<(.*?)>", "").Contains(filterText))
                            {
                                newTexts.Add(textGroup);
                                textGroup.PreviewLine = TrimAroundPhrase(line, filterText);
                                break;
                            }
                        }
                    }
                    i++;
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        progressWindow.Bar.Value = i;
                        progressWindow.progress.Text = i.ToString() + "/" + linkdata.Dialogues.Count.ToString();
                    });
                }
            });
            window.IsHitTestVisible = true;
            progressWindow.Close();
            MenuTexts = newTexts;
            OnPropertyChanged(nameof(MenuTexts));
        }

        public async void TextFilter()
        {
            var newDialogues = new ObservableCollection<Dialogue>();
            var progressWindow = new ProgressWindow("Searching for string...", "Note: Things like names, player pronouns or other generated text wont filter properly.", (uint)linkdata.Dialogues.Count);
            progressWindow.Show();
            var filterText = FilterText?.Trim().ToLower() ?? String.Empty;

            window.IsHitTestVisible = false;
            await Task.Run(() =>
            {
                int i = 0;
                foreach (var dialogue in linkdata.Dialogues)
                {
                    var mad = dialogue.GetTextLinesPreview().ToList();
                    foreach (var line in mad)
                    {
                        var newline = line?.Trim().ToLower() ?? String.Empty;
                        if (!String.IsNullOrEmpty(newline) && !newline.Equals("\0"))
                        {
                            if (newline.Contains(filterText) || Regex.Replace(newline, @"<(.*?)>", "").Contains(filterText))
                            {
                                newDialogues.Add(dialogue);
                                dialogue.PreviewLine = TrimAroundPhrase(line, filterText);
                                break;
                            }
                        }
                    }
                    i++;
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        progressWindow.Bar.Value = i;
                        progressWindow.progress.Text = i.ToString() + "/" + linkdata.Dialogues.Count.ToString();
                    });
                }
            });
            window.IsHitTestVisible = true;
            progressWindow.Close();
            Dialogues = newDialogues;
            OnPropertyChanged(nameof(Dialogues));
        }

        public void SizeChange()
        {
            OnPropertyChanged(nameof(EntryWidth));
        }

        string TrimAroundPhrase(string text, string phrase)
        {
            int wordsAround = 3;
            var words = text.Split(' ');
            var phraseWords = phrase.Split(' ');

            for (int i = 0; i <= words.Length - phraseWords.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < phraseWords.Length; j++)
                {
                    if (!words[i + j].Contains(phraseWords[j], StringComparison.OrdinalIgnoreCase))
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    int start = Math.Max(0, i - wordsAround);
                    int end = Math.Min(words.Length - 1, i + phraseWords.Length - 1 + wordsAround);
                    var trimmed = words.Skip(start).Take(end - start + 1);
                    string result = string.Join(" ", trimmed);

                    if (start > 0) result = "… " + result;
                    if (end < words.Length - 1) result += " …";
                    return result;
                }
            }
            return ""; // Phrase not found
        }

        public void SelectedToEdit()
        {
            IsEditing = true;
            EditingLine = null;
            EditingTextGroup = SelectedTextGroup;
            if (!(EditingTextGroup is Dialogue) || linkdata.Encrypted) UpdateEditText();
            else
            {
                UpdateEditDialogue();
            }

        }
        public void EditToSelected()
        {
            EditingTextGroup.UpdateLines(TextLinesEdit.ToArray());
            Dialogues = linkdata.Dialogues;
            MenuTexts = linkdata.MenuTexts;
            OnPropertyChanged(nameof(Dialogues));
            OnPropertyChanged(nameof(MenuTexts));
        }
        public void UpdateEditDialogue()
        {
            DialogueLinesEdit.Clear();
            if (EditingTextGroup == null) return;
            var mad = ((Dialogue)EditingTextGroup).GetDialogueLines().ToList();
            foreach (var line in mad)
            {
                DialogueLinesEdit.Add(line);
            }
            //PreviewIndex = SelectedTextGroup.TextDataIndex;
            //OnPropertyChanged(nameof(PreviewIndex));
            //_dialogue = SelectedTextGroup is Dialogue;
            //OnPropertyChanged(nameof(DialogueHeight));
        }

        public void UpdateEditText()
        {
            TextLinesEdit.Clear();
            if(EditingTextGroup == null) return;
            var mad = EditingTextGroup.GetTextLinesPreview().ToList();
            foreach (var line in mad)
            {
                TextLinesEdit.Add(line);
            }
            //PreviewIndex = SelectedTextGroup.TextDataIndex;
            //OnPropertyChanged(nameof(PreviewIndex));
            //_dialogue = SelectedTextGroup is Dialogue;
            //OnPropertyChanged(nameof(DialogueHeight));
        }

        //EDITING TEXT LINE
        private bool _editing = false;
        public bool IsBrowsing { 
            get => !_editing; 
            set
            {
                if (_editing == value)
                {
                    _editing = !value;
                    OnPropertyChanged(nameof(IsBrowsing));
                    OnPropertyChanged(nameof(IsEditing));
                    OnPropertyChanged(nameof(IsBrowsingV));
                    OnPropertyChanged(nameof(IsEditingV));
                }
            }
        }
        public bool IsEditing
        {
            get => _editing;
            set
            {
                if (_editing != value)
                {
                    _editing = value;
                    OnPropertyChanged(nameof(IsBrowsing));
                    OnPropertyChanged(nameof(IsEditing));
                    OnPropertyChanged(nameof(IsBrowsingV));
                    OnPropertyChanged(nameof(IsEditingV));
                }
            }
        }

        public Visibility IsBrowsingV => !_editing ? Visibility.Visible : Visibility.Hidden;
        public Visibility IsEditingV => _editing ? Visibility.Visible : Visibility.Hidden;
        private String _editingLine;
        public String EditingLine
        {
            get => _editingLine; set
            {
                _editingLine = value;
                OnPropertyChanged(nameof(EditingLine));
            }
        }
        //---------------------------------------

        public (List<List<string>>,List<bool>) GetNPCFiles()
        {
            TextGroup names = linkdata.MenuTexts[21];
            var res = names.GetTextLinesPreviewAllLang();
            List<bool> asi = new List<bool>();
            for (int i = 0; i < res.Count; i++)
            {
                asi.Add(linkdata.AsianLanguages.Contains((byte)i));
            }
            return (res, asi);
        }

        //--------------------------------------------

        //Exporting

        public async void ExportAllLinesFrom(int[] CharID, string path)
        {
            var newDialogues = new ObservableCollection<Dialogue>();
            var progressWindow = new ProgressWindow("Searching for all lines...", "This might take a while.", (uint)linkdata.Dialogues.Count);
            progressWindow.Show();
            var filterText = FilterText?.Trim().ToLower() ?? String.Empty;
            File.WriteAllText(path,"");
            window.IsHitTestVisible = false;
            await Task.Run(() =>
            {
                int i = 0;
                foreach (var dialogue in linkdata.Dialogues)
                {
                    var mad = dialogue.GetDialogueLines().ToList();
                    foreach (var entry in mad)
                    {
                        var arg = entry.GetArgument(0);
                        int id = (int)arg.Item1;
                        if (CharID.Contains(id))
                        {
                            if (!String.IsNullOrEmpty(entry.Line) && !entry.Line.Equals("\0"))
                                File.AppendAllText(path, "=-=-=-=-=-=-=-=-=-=-=-= " + id + " =-=-=-=-=-=-=-=-=-=-=-=\n" + TextBlockExtensions.ProcessLine(entry.Line) + "\n");
                        }
                    }
                    i++;
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        progressWindow.Bar.Value = i;
                        progressWindow.progress.Text = i.ToString() + "/" + linkdata.Dialogues.Count.ToString();
                    });
                }
            });
            window.IsHitTestVisible = true;
            progressWindow.Close();
            OnPropertyChanged(nameof(Dialogues));
        }

        public async void ExtractCommandArguments(string path)
        {
            var theDictionary = new Dictionary<ushort, List<int>[]>();
            var progressWindow = new ProgressWindow("Searching for all lines...", "This might take a while.", (uint)linkdata.Dialogues.Count);
            progressWindow.Show();

            File.WriteAllText(path, "");
            window.IsHitTestVisible = false;
            await Task.Run(() =>
            {
                int i = 0;
                foreach (var dialogue in linkdata.Dialogues)
                {
                    var mad = dialogue.GetDialogueLines().ToList();
                    foreach (var entry in mad)
                    {
                        if(!theDictionary.ContainsKey(entry.command))
                            theDictionary.Add(entry.command, new List<int>[11]);
                        for(int argIndex = 0; argIndex < 11; argIndex++)
                        {
                            var arg = entry.GetArgument(argIndex);
                            if((int)arg.Item1 != -1)
                            {
                                if (theDictionary[entry.command][argIndex] == null)
                                    theDictionary[entry.command][argIndex] = new List<int>();
                                if (!theDictionary[entry.command][argIndex].Contains((int)arg.Item1))
                                    theDictionary[entry.command][argIndex].Add((int)arg.Item1);
                            }
                        }
                    }
                    i++;
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        progressWindow.Bar.Value = i;
                        progressWindow.progress.Text = i.ToString() + "/" + linkdata.Dialogues.Count.ToString();
                    });
                }
            });
            var a = theDictionary.Keys.ToList();
            a.Sort();
            foreach (var ke in a)
            {
                var cmd = theDictionary[ke];
                File.AppendAllText(path, "\n================ Command " + ke + " ================\n");
                for(int argIndex = 0; argIndex < cmd.Length; argIndex++)
                {
                    var argList = cmd[argIndex];
                    if(argList != null)
                    {
                        File.AppendAllText(path, "-- Argument " + argIndex + " --\n");
                        argList.Sort();
                        foreach (var arg in argList)
                        {
                            File.AppendAllText(path, arg.ToString() + ", ");
                        }
                        File.AppendAllText(path,"\n");
                    }
                }
            }
            window.IsHitTestVisible = true;
            progressWindow.Close();
        }


    }
}
