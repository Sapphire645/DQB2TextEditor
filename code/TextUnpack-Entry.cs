using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DQB2TextEditor.code
{
	public class TextUnpack
	{
        private readonly ushort PointerToPointerStart = 0x40;
        private readonly ushort EntrySize = 0x34;

        private int PointersStart => PointerToPointerStart + EntryCountPointerStart * 4;
        public ObservableCollection<DialogueEntry> Text { get; private set; } = new ObservableCollection<DialogueEntry>();
        public ObservableCollection<TextEntry> RawTextAttempt { get; private set; } = new ObservableCollection<TextEntry>();
        public String RawText { get; set; }
        private int EntryCount = 0;
        private int EntryCountPointerStart = 0;

        public bool IsCutscene => CutsceneFile != null;
        public bool RawTextShow => EntryCount > 1024;
        private byte[] CutsceneFile;
        private byte[] TextFile;

        public byte[] GetTextFile => TextFile;
        public byte[] GetCutsceneFile => CutsceneFile;
        public void SaveFlowData(String path)
        {
            System.IO.File.WriteAllBytes(path+"old.unpack", CutsceneFile);
            EntryToBytes();
            System.IO.File.WriteAllBytes(path, CutsceneFile);
        }
        public void SaveTextData(String path)
        {
            System.IO.File.WriteAllBytes(path + "old.unpack", TextFile);
            EntryToBytes();
            System.IO.File.WriteAllBytes(path, TextFile);
        }
        public void SaveTxtFile(String path, bool processed)
        {
            System.IO.File.Delete(path);
            foreach (DialogueEntry EntryVisual in Text)
            {
                Entry Entry = EntryVisual.Entry.Value;
                String Append = "";
                if (CutsceneFile != null)
                    Append = Entry.FullCommand.Trim() + "\t";
                if (processed)
                {
                    StringBuilder stringBuilder = new StringBuilder(Append); //Alright fiine I'll use stringbuilder

                    foreach (Inline inline in Entry.LineProcess().Inlines)
                    {
                        if (inline is Run run)
                            stringBuilder.Append(run.Text);
                        else
                            if(inline is InlineUIContainer contain) //Might add a value to switch to the katakana
                                stringBuilder.Append(((TextBlock)((Grid)contain.Child).Children[1]).Text); 
                    }
                    System.IO.File.AppendAllText(path, stringBuilder.ToString() + "\n", Encoding.UTF8);
                }
                else
                    System.IO.File.AppendAllText(path, Append + Entry.Line+"\n", Encoding.UTF8);
            }
            
        }
        public TextUnpack(byte[] cutsceneFile, byte[] textFile)
		{
            TextFile = textFile;
            CutsceneFile = cutsceneFile;
            BytesToEntryDialogue();
        }
        public TextUnpack(byte[] cutsceneFile, byte[] textFile, ObservableCollection<DialogueEntry> recycle, bool value)
        {
            TextFile = textFile;
            CutsceneFile = cutsceneFile;
            Text = recycle;
            BytesToEntryDialogue();
            SwapPreview(value);
        }

        public TextUnpack(byte[] textFile, bool value)
        {
            TextFile = textFile;
            CutsceneFile = null;
            BytesToEntryText();
            SwapPreview(value);
        }
        public TextUnpack(byte[] textFile, ObservableCollection<TextEntry> recycle, bool value)
        {
            TextFile = textFile;
            CutsceneFile = null;
            RawTextAttempt = recycle;
            BytesToEntryText();
            SwapPreview(value);
        }
        private void CheckEntryCount()
        {
            var StartTextPointer = BitConverter.ToInt32(TextFile, EntryCountPointerStart * 4 + PointerToPointerStart);
            StartTextPointer += EntryCountPointerStart * 4 + PointerToPointerStart;

            EntryCount = (StartTextPointer - (EntryCountPointerStart * 4 + PointerToPointerStart))/ 4 ;
        }

        private void BytesToEntryText()
        {
            EntryCountPointerStart = BitConverter.ToInt32(TextFile, 0);
            CheckEntryCount();
            if (EntryCount > 1024) //It dies.
            {
                for (int pointerOffset = 0; pointerOffset < EntryCount; pointerOffset++)
                {
                    RawText += pointerOffset + ":\t" + getTextFromPointerOffset(pointerOffset) + "\n";
                }
            }
            else
            {
                if (RawTextAttempt.Count > EntryCount)
                    RawTextAttempt = new ObservableCollection<TextEntry>(RawTextAttempt.Take(EntryCount));
                var count = RawTextAttempt.Count;
                for (int pointerOffset = 0; pointerOffset < EntryCount; pointerOffset++)
                {
                    var EntryVar = new Entry(getTextFromPointerOffset(pointerOffset), pointerOffset);
                    if (pointerOffset < count)
                        RawTextAttempt[pointerOffset].UpdateNewText(EntryVar);
                    else
                        RawTextAttempt.Add(new TextEntry(EntryVar));
                }
            }
        }
        private void BytesToEntryDialogue()
        {
            EntryCountPointerStart = BitConverter.ToInt32(TextFile, 0);
            CheckEntryCount();
            //Trying to optimize this. Takes too long.
            if(Text.Count >= EntryCount)
                Text = new ObservableCollection<DialogueEntry>(Text.Take(EntryCount));
            var Count = Text.Count;

            for (int pointerOffset = 0; pointerOffset < EntryCount; pointerOffset++)
            {
                var EntryVar = new Entry(getTextFromPointerOffset(pointerOffset), pointerOffset);
                EntryVar = UpdateEntry(EntryVar);
                if (pointerOffset < Count)
                    Text[pointerOffset].UpdateNewText(EntryVar);
                else
                {
                    DialogueEntry var = new DialogueEntry(EntryVar);
                    var.PButton.Click += AddEntry;
                    var.MButton.Click += RemoveEntry;
                    Text.Add(var);
                }

            }
        }
        private void AddEntry(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            int index = (int)button.CommandParameter;
            bool lastNull = false;
            DialogueEntry entry = new DialogueEntry(Text[index].Entry.Value.Clone());
            entry.PButton.Click += AddEntry;
            entry.MButton.Click += RemoveEntry;
            Text.Insert(index + 1, entry);
            for(int i = index+1; i < Text.Count; i++)
            {
                if (Text[i].Entry.Value.Arguments == null)
                    if (lastNull)
                    {
                        Text.RemoveAt(i);
                        break;
                    }
                    else
                        lastNull = true;
                else
                    lastNull = false;
                Text[i].Entry.Value.PointerOffset++;
                Text[i].Entry.NotifyValue();
            }
        }
        private void RemoveEntry(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            int index = (int)button.CommandParameter;
            bool lastNull = false;
            Text.RemoveAt(index);
            for (int i = index; i < Text.Count; i++)
            {
                if (Text[i].Entry.Value.Arguments == null)
                    if (lastNull)
                    {
                        DialogueEntry entry = new DialogueEntry(Text[i].Entry.Value.Clone());
                        entry.PButton.Click += AddEntry;
                        entry.MButton.Click += RemoveEntry;
                        Text.Insert(i, entry);
                        Text[i].Entry.Value.PointerOffset--;
                        Text[i].Entry.NotifyValue();
                        break;
                    }
                    else
                        lastNull = true;
                else
                    lastNull = false;
                Text[i].Entry.Value.PointerOffset--;
                Text[i].Entry.NotifyValue();
            }
        }
        public void EntryToBytes()
        {
            var EntryCountBytes = BitConverter.GetBytes(EntryCountPointerStart); //Might have to edit this...
            Array.Copy(EntryCountBytes, 0, TextFile, 0, 4);

            if (CutsceneFile != null)
                if (VersionInformation.Encrypted)
                    foreach (DialogueEntry EntryVisual in Text)
                    {
                        Entry Entry = EntryVisual.Entry.Value;
                        setTextFromEntry(Entry);
                    }
                else
                    foreach (DialogueEntry EntryVisual in Text)
                    {
                        Entry Entry = EntryVisual.Entry.Value;
                        setTextFromEntry(Entry);
                        setCutsceneFromEntry(Entry);
                    }
            else{
                if (RawTextShow)
                {
                    foreach (String EntryVisual in RawText.Split("\n"))
                    {
                        var Val = EntryVisual.Split(":\t");
                        Entry Entry = new Entry(Val[1], int.Parse(Val[0]));
                        setTextFromEntry(Entry);
                    }
                }
                else
                    foreach (TextEntry EntryVisual in RawTextAttempt)
                    {
                        Entry Entry = EntryVisual.Entry.Value;
                        setTextFromEntry(Entry);
                    }
            }

        }

        private Entry UpdateEntry(Entry entry) {
            if (entry.PointerOffset >= CutsceneFile.Length / EntrySize) return entry;
            int[] Arguments = new int[11];
            for (int ArgIndex = 0; ArgIndex < 11; ArgIndex++)
                Arguments[ArgIndex] = BitConverter.ToInt32(CutsceneFile, entry.PointerOffset* EntrySize + ArgIndex*4);
            entry.Arguments = Arguments;
            entry.Command = BitConverter.ToUInt16(CutsceneFile, entry.PointerOffset * EntrySize + 0x2E);
            entry.LastArgument = BitConverter.ToInt32(CutsceneFile, entry.PointerOffset * EntrySize + 0x30) == 1;
            return entry;
        }
        private void setCutsceneFromEntry(Entry entry)
        {
            if (entry.Arguments == null)
            {
                if (CutsceneFile == null || CutsceneFile.Length <= entry.PointerOffset*EntrySize) return;
                byte[] CutBytes = new byte[(entry.PointerOffset) * EntrySize];
                Array.Copy(CutsceneFile, CutBytes, CutBytes.Length);
                CutsceneFile = CutBytes;
                return;
            }
            if (entry.PointerOffset*EntrySize >= CutsceneFile.Length)
            {
                byte[] NewBytes = new byte[(entry.PointerOffset+1)*EntrySize];
                Array.Copy(CutsceneFile, NewBytes, CutsceneFile.Length);
                CutsceneFile = NewBytes;
            }
            for (int ArgIndex = 0; ArgIndex < 11; ArgIndex++)
            {
                var ArgInt = BitConverter.GetBytes(entry.Arguments[ArgIndex]);
                Array.Copy(ArgInt,0,CutsceneFile, entry.PointerOffset * EntrySize + ArgIndex * 4,4);
            }
            var CommandInt = BitConverter.GetBytes(entry.Command);
            Array.Copy(CommandInt,0, CutsceneFile, entry.PointerOffset * EntrySize + 0x2E,2);
            int last = 0;
            if (entry.LastArgument) last = 1;
            var LastInt = BitConverter.GetBytes(last);
            Array.Copy(LastInt, 0, CutsceneFile, entry.PointerOffset * EntrySize + 0x30, 4);
            return;
        }

        private String getTextFromPointerOffset(int offset)
        {
            if(offset < EntryCount-1)
            {
                int PointerCurrent = BitConverter.ToInt32(TextFile, PointersStart + offset * 4);
                int PointerNext = BitConverter.ToInt32(TextFile, PointersStart + (offset + 1) * 4);
                if (PointerCurrent + PointersStart + offset * 4 < 0 || PointerNext + 4 - PointerCurrent < 0) return "";
                return System.Text.Encoding.UTF8.GetString(TextFile, PointerCurrent + PointersStart + offset * 4, PointerNext + 4 - PointerCurrent);

            }
            else
            {
                int PointerCurrent = BitConverter.ToInt32(TextFile, PointersStart + offset * 4);
                if (TextFile.Length - (PointerCurrent + PointersStart + offset * 4) < 0 || PointerCurrent + PointersStart + offset * 4 < 0) return "";
                return System.Text.Encoding.UTF8.GetString(TextFile, PointerCurrent + PointersStart + offset * 4, TextFile.Length -(PointerCurrent + PointersStart + offset * 4));
            }
        }
        private void setTextFromEntry(Entry Entry)
        {
            var offset = Entry.PointerOffset;
            var text = Encoding.UTF8.GetBytes(Entry.Line);
            var textSize = text.Length;
            int PointerCurrent = BitConverter.ToInt32(TextFile, PointersStart + offset * 4);

            if (offset < EntryCount - 1)
            {
                byte[] PointerNext = BitConverter.GetBytes(PointerCurrent + textSize - 4);
                //set next pointer
                Array.Copy(PointerNext, 0, TextFile, PointersStart + (offset + 1) * 4, 4);
            }
            if(textSize + PointerCurrent + PointersStart + offset * 4 > TextFile.Length)
            {
                byte[] ExtBytes = new byte[textSize + PointerCurrent + PointersStart + offset * 4];
                Array.Copy(TextFile, ExtBytes, TextFile.Length);
                TextFile = ExtBytes;
                return;
            }
            Array.Copy(text, 0, TextFile, PointerCurrent + PointersStart + offset * 4, textSize);
        }

        public void SwapPreview(bool value)
        {
            foreach (DialogueEntry EntryVisual in Text)
            {
                EntryVisual.Preview = value;
                if (value) EntryVisual.Entry.NotifyValue();
            }
        }
        public void UpdateFlowData(byte[] flowData){
            if (VersionInformation.Encrypted) return;
            EntryToBytes();
            CutsceneFile = flowData;
            BytesToEntryDialogue();
        }
        public void UpdateTxtData(byte[] txtData)
        {
            EntryToBytes();
            TextFile = txtData;
            BytesToEntryText();
        }
        public void UpdateTextString(string path)
        {
            String[] lines = System.IO.File.ReadAllLines(path);
            if (lines[0].Contains("\t") && !IsCutscene) return;
            if (!lines[0].Contains("\t") && IsCutscene) return;
            if (lines[0].Contains("(Encrypted)\t") && !VersionInformation.Encrypted) return; //pain
            if (!lines[0].Contains("(Encrypted)\t") && VersionInformation.Encrypted) return; //agony even

            EntryCount = lines.Length;
            //Trying to optimize this. Takes too long.
            if (Text.Count >= EntryCount)
                Text = new ObservableCollection<DialogueEntry>(Text.Take(EntryCount));
            var Count = Text.Count;
            int pointerOffset = 0;
            foreach (string line in lines)
            {
                string[] Values = line.Split("\t");
                Entry EntryVar = null;
                if (Values.Length == 1)
                    EntryVar = new Entry(Values[0], pointerOffset);
                else if (Values.Length == 2)
                    EntryVar = new Entry(Values[0], Values[1], pointerOffset);
                if (pointerOffset < Count)
                    Text[pointerOffset].UpdateNewText(EntryVar);
                else
                {
                    DialogueEntry var = new DialogueEntry(EntryVar);
                    var.PButton.Click += AddEntry;
                    var.MButton.Click += RemoveEntry;
                    Text.Add(var);
                }
                pointerOffset++;
            }
        }
    }

    public class Entry
    {
        public string Line { get; set; }
        public int PointerOffset { get; set; }

        public int[] Arguments { get; set; }
        public uint Command { get; set; }
        public bool LastArgumentVisual { get { return LastArgument; }  set { LastArgument = !LastArgument; } }
        public bool LastArgument { get; set; }
        public string Name => VersionInformation.JapaneseMode ? null : ShowName != null ? ShowName : GetName();
        public Visibility NameVisible => Name != null ? Visibility.Visible : Visibility.Collapsed;
        private string ShowName { get{
                if (Line.Contains("<show("))
                    return Regex.Match(Line, @"<show\((.*?)\)>").Groups[1].Value;
                return null;
            } set { } }
        public System.Windows.Media.SolidColorBrush Background
        {
            get
            {
                if (Line.Contains("<off>"))
                    return System.Windows.Media.Brushes.Transparent;
                return System.Windows.Media.Brushes.Black;
            }
            set { }
        }
        private string GetName()
        {
            if(Command == 110 || Command == 111){
                if (Arguments != null && Arguments[0] != 0)
                    return VersionInformation.NamesPreview[Arguments[0]-1];
                return null;
            }
            return null;
        }
        public Paragraph LineProcess()
        {
            var LineProcessed = Line.Replace("<6>", "‛");
            LineProcessed = LineProcessed.Replace("<9>", "’");
            if (VersionInformation.JapaneseMode)
            {
                var name = ShowName != null ? ShowName : GetName();
                if (name != null && name.Length > 1){
                    LineProcessed = name + "「" + LineProcessed;
                    LineProcessed = LineProcessed.Replace("<br>", Environment.NewLine + "    ");
                }
                else
                {
                    LineProcessed = LineProcessed.Replace("<br>", Environment.NewLine);
                }
            }
            else
            {
                LineProcessed = LineProcessed.Replace("<br>", Environment.NewLine);
            }

            LineProcessed = LineProcessed.Replace("<key>", "");

            LineProcessed = LineProcessed.Replace("<scron>", "");
            LineProcessed = LineProcessed.Replace("<scroff>", "");
            LineProcessed = LineProcessed.Replace("<off>", "");
            LineProcessed = LineProcessed.Replace("<-->", "─");
            LineProcessed = LineProcessed.Replace("<--->", "⎯⎯ ");
            LineProcessed = LineProcessed.Replace("<note>", "♩");
            LineProcessed = LineProcessed.Replace("<pname>", VersionInformation.PlayerNameDefault);
            LineProcessed = Regex.Replace(LineProcessed, @"<show\((.*?)\)>", ""); //Name
            LineProcessed = Regex.Replace(LineProcessed, @"<\$iicon\((.*?)\)>", "⧉"); //Icon
            LineProcessed = Regex.Replace(LineProcessed, @"<\$kicon\((.*?)\)>", "⧉"); //Icon
            LineProcessed = Regex.Replace(LineProcessed, @"<\$icon\((.*?)\)>", "⧉"); //Icon
            LineProcessed = Regex.Replace(LineProcessed, @"<button\((.*?)\)>", "◎"); //Button
            LineProcessed = Regex.Replace(LineProcessed, @"<\$ui\((.*?)\)>", "Ⓤ"); //

            LineProcessed = Regex.Replace(LineProcessed, @"<morf\((.*?),(.*?)\)>", match => match.Groups[VersionInformation.PlayerGender ? 2 : 1].Value);
            LineProcessed = Regex.Replace(LineProcessed, @"<\$iname\((\d+)\)>", match => VersionInformation.ItemsPreview[int.Parse(match.Groups[1].Value)]);
            LineProcessed = Regex.Replace(LineProcessed, @"<\$sgl_iname\((\d+)\)>", match => VersionInformation.ItemsPreview[int.Parse(match.Groups[1].Value)] );

            LineProcessed = Regex.Replace(LineProcessed, @"<\$plr_iname\((\d+)\)>", match => VersionInformation.ItemsPreview[int.Parse(match.Groups[1].Value)]+"(/s)");

            LineProcessed = Regex.Replace(LineProcessed, @"<\$plr_kbname\((\d+)\)>", match => "(kname " + match.Groups[1].Value + "s)");
            LineProcessed = Regex.Replace(LineProcessed, @"<\$sgl_kname\((\d+)\)>", match => "(kname " + match.Groups[1].Value + ")");
            LineProcessed = Regex.Replace(LineProcessed, @"<\$kname\((\d+)\)>", match => "(kname " + match.Groups[1].Value + ")");
            LineProcessed = Regex.Replace(LineProcessed, @"(?<=<cap>)[a-zA-Z]", match => match.Value.ToUpper());
            LineProcessed = LineProcessed.Replace("<cap>", "");

            LineProcessed = Regex.Replace(LineProcessed, @"<allcap>(.*?)</allcap>", match => match.Groups[1].Value.ToUpper()); //allcap

            var Paragraph = new Paragraph();
            //Colour
            var ColourLines = Regex.Split(LineProcessed, $"(?={Regex.Escape(@"</color>") + "|" + Regex.Escape(@"<$cdef(")})");
            foreach (var ColourText in ColourLines)
            {
                if (ColourText.StartsWith("<$cdef(") && int.TryParse(Regex.Match(Line, @"<\$cdef\((\d+)\)>").Groups[1].Value, out var ColourNumber))
                {
                    var Line = Regex.Replace(ColourText, @"<\$cdef\((.*?)\)>", "");
                    BrushConverter brushConverter = new BrushConverter();
                    ProcessLineJp(Paragraph, Line, (System.Windows.Media.Brush)brushConverter.ConvertFromString(VersionInformation.ColourPreview[ColourNumber]));
                }
                else
                {
                    var Line = ColourText.Replace("</color>", "");
                    ProcessLineJp(Paragraph, Line, System.Windows.Media.Brushes.White);
                }
            }
            return Paragraph;
        }

        private void ProcessLineJp(Paragraph paragraph, string Line, System.Windows.Media.Brush BG)
        {
            var JPLines = Regex.Split(Line, @"(<[^>]+:[^>]+>)"); 
            for (ushort i = 0;  i < JPLines.Length; i++)
            {
                if(Regex.IsMatch(JPLines[i], @"<(.*?):(.*?)>"))
                {
                    var stringMatch = Regex.Match(JPLines[i], @"<(.*?):(.*?)>");

                    var inlineUIContainer1 = new InlineUIContainer();
                    var Grid = new Grid() { Height = 31.5 };

                    //Top Text
                    var textBlockTop = new TextBlock { Text = stringMatch.Groups[2].Value, FontSize = 10, Foreground=BG, Background = System.Windows.Media.Brushes.Transparent, HorizontalAlignment = System.Windows.HorizontalAlignment.Center};
                    Grid.Children.Add(textBlockTop);
                    //Bottom text
                    var textBlockBottom = new TextBlock { Text = stringMatch.Groups[1].Value, FontSize = 21, Foreground = BG, Background = System.Windows.Media.Brushes.Transparent, Margin = new Thickness(0,8,0,0) };

                    Grid.Children.Add(textBlockBottom);

                    inlineUIContainer1.Child = Grid;
                    paragraph.Inlines.Add(inlineUIContainer1);
                }
                paragraph.Inlines.Add(new Run(Regex.Replace(JPLines[i], @"<(.*?):(.*?)>", ""))
                {
                    Foreground = BG
                });
            }
        }
        public String FullCommand { get {
                if (Arguments == null) return "null";
                if (VersionInformation.Encrypted) return "(Encrypted)";
                bool Coma = false;
                String Temp;
                CommandInfo CommandData = VersionInformation.FindCommand(Command);
                if (CommandData != null)
                    Temp = CommandData.NameFull;
                else
                    Temp = "*: " + Command;
                Temp += "(";
                for(int i = 0; i < Arguments.Length; i++)
                {
                    if (Arguments[i] != -1)
                    {
                        if (Coma) Temp += ",";
                        if (CommandData != null && CommandData.Arguments[i] != null)
                            Temp += CommandData.Arguments[i] + ":" + Arguments[i];
                        else
                            Temp += i + ":" + Arguments[i];
                        Coma = true;
                    }
                }
                Temp += ")";
                return Temp;

            } set
            {

            }
        }
        private void unpackCommand(string command)
        {
            //hnnn
        }
        public Entry(string line, string command, int pointerOffset)
        {
            Line = line;
            PointerOffset = pointerOffset;
            unpackCommand(command);
        }
        public Entry(string line,int pointerOffset)
        {
            Line = line;
            PointerOffset = pointerOffset;
        }
        public Entry Clone()
        {
            Entry NewVal = new Entry(Line, PointerOffset);
            NewVal.Command = Command;
            if(NewVal.Arguments != null)
            {
                NewVal.Arguments = new int[11];
                Arguments.CopyTo(NewVal.Arguments, 0);
            }
            return NewVal;
        }
    }
}
