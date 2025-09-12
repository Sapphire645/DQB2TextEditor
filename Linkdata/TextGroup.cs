using DQB2TextEditor.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DQB2TextEditor.Linkdata
{
    public class TextGroup : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private LINKDATAEntry[] _TextDataFiles;
        public ushort TextDataIndex => _TextDataFiles[ViewModel._currentLanguage].Index;
        public UInt64 TextDataOffset => _TextDataFiles[ViewModel._currentLanguage].Offset;
        public UInt64 TextDataUncompressedSize => _TextDataFiles[ViewModel._currentLanguage].UncompressedSize;
        public UInt64 TextDataCompressedSize => _TextDataFiles[ViewModel._currentLanguage].CompressedSize;

        public string PreviewLine { get; set; } = "";
        protected LINKDATAEntry TextDataFile => _TextDataFiles[ViewModel._currentLanguage];

        private ushort _index;
        public ushort Index => _index;
        public TextGroup(LINKDATAEntry[] textDataFiles, ushort index)
        {
            ViewModel.me.PropertyChanged += UpdateLanguage;
            _TextDataFiles = textDataFiles;
            _index = index;
        }
        public void UpdateLanguage(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel._currentLanguage))  // BLOCKINFO CHANGE
            {
                OnPropertyChanged(nameof(TextDataIndex));
                OnPropertyChanged(nameof(TextDataOffset));
                OnPropertyChanged(nameof(TextDataUncompressedSize));
                OnPropertyChanged(nameof(TextDataCompressedSize));
            }
        }
        public ObservableCollection<string> GetTextLinesPreview()
        {
            ObservableCollection<string> textLines = new ObservableCollection<string>(
                ((LDFolder_TextData)TextDataFile.LINKDATAData).GetTextLinesPreview()
                );

            return textLines;
        }
    }
}
