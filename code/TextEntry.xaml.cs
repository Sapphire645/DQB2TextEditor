using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DQB2TextEditor.code
{
    public partial class TextEntry : System.Windows.Controls.UserControl
    {
        public ObservableProperty<Entry> Entry { get; set; } = new ObservableProperty<Entry>();
        private WeakReference<System.Windows.Controls.RichTextBox> Rich;
        public TextEntry(Entry entry)
        {
            Entry.Value = entry;
            InitializeComponent();
            DataContext = this;
        }
        public void UpdateNewText(Entry entry)
        {
            Entry.Value = entry;
            UGrid.Children.Clear();
        }

        private void PreviewShow(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.RichTextBox RichText;
            if (Rich == null || !Rich.TryGetTarget(out RichText))
            {
                UGrid.Children.Clear();
                RichText = new System.Windows.Controls.RichTextBox()
                {
                    Background = Entry.Value.Background,
                    BorderThickness = new Thickness(2),
                    IsReadOnly = true,
                    Padding = new Thickness(14,6,14,6),
                    FontSize = 21
                };
                Rich = new WeakReference<System.Windows.Controls.RichTextBox>(RichText);
            }
            RichText.Document.Blocks.Clear();
            RichText.Document.Blocks.Add(Entry.Value.LineProcess());
            UGrid.Children.Add(RichText);
        }

        private void PreviewHide(object sender, RoutedEventArgs e)
        {
            UGrid.Children.Clear();
        }
    }
}
