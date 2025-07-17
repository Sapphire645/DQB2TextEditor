using DQB2TextEditor.Linkdata;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media.TextFormatting;

namespace DQB2TextEditor.Windows
{
    internal class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public static ViewModel me;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static LINKDATA linkdata { get; private set; }

        public byte CurrentLanguage
        {
            get { return _currentLanguage; }
            set
            {
                if (_currentLanguage != value)
                {
                    _currentLanguage = value;
                    OnPropertyChanged(nameof(CurrentLanguage));
                    OnPropertyChanged(nameof(SelectedTextGroup));
                    OnPropertyChanged(nameof(_currentLanguage));
                }
            }
        }

        public static byte _currentLanguage = 0;
        public bool selected => _selectedTextGroup != null;
        public ObservableCollection<Dialogue> Dialogues => linkdata.Dialogues;
        public ObservableCollection<String> TextLinesPreview { get; private set; } = new ObservableCollection<string>() { "aa", "bb" };

        private TextGroup _selectedTextGroup;

        private TextEditorWindow window;
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
                    OnPropertyChanged(nameof(Selection));
                    OnPropertyChanged(nameof(selected));
                }
            }
        }
        public string Selection => SelectedTextGroup is Dialogue ? "Dialogue" : "Text";


        public ViewModel(SLViewModel SLVM, TextEditorWindow window)
        {
            me = this;
            this.window = window;
            linkdata = SLVM.CreateLINKDATA();
        }


        public void UpdatePreviewText()
        {
            TextLinesPreview.Clear();
            var mad = SelectedTextGroup.GetTextLinesPreview().ToList();
            foreach(var line in mad){
                if (!String.IsNullOrEmpty(line) && !line.Equals("\0"))
                    TextLinesPreview.Add(line);
            }
        }

        public void UpdateWidth()
        {
            OnPropertyChanged(nameof(TextWidth));
        }
    }
}
