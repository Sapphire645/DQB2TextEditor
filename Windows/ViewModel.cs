using DQB2TextEditor.Linkdata;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.TextFormatting;

namespace DQB2TextEditor.Windows
{
    internal class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public LINKDATA linkdata { get; private set; }

        public byte CurrentLanguage { get { return _currentLanguage; } set { if (_currentLanguage != value)
                {
                    _currentLanguage = value;
                    OnPropertyChanged(nameof(CurrentLanguage));
                    OnPropertyChanged(nameof(SelectedTextGroup));
                }
            } }

        public static byte _currentLanguage = 0;
        public ObservableCollection<Dialogue> Dialogues => linkdata.Dialogues;
        public ObservableCollection<String> TextLinesPreview { get; private set; } = new ObservableCollection<string>() {"aa","bb"};

        private TextGroup _selectedTextGroup;
        public TextGroup SelectedTextGroup { 
            get { return _selectedTextGroup; } 
            set{ if (_selectedTextGroup != value) {
                    _selectedTextGroup = value;
                    OnPropertyChanged(nameof(SelectedTextGroup));
                    OnPropertyChanged(nameof(Selection));
                } 
            } }
        public string Selection => SelectedTextGroup is Dialogue? "Dialogue" : "Text";


        public ViewModel(LINKDATA linkdata)
        {
            this.linkdata = linkdata;
        }

    }
}
