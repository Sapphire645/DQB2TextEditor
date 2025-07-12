using DQB2TextEditor.Linkdata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DQB2TextEditor.Windows
{
    /// <summary>
    /// Interaction logic for TextEditor.xaml
    /// </summary>
    public partial class TextEditorWindow : Window
    {
        private ViewModel viewModel;
        public TextEditorWindow(LINKDATA linkdata)
        {
            viewModel = new ViewModel(linkdata);
            DataContext = viewModel;
            InitializeComponent();

        }

        private void Dialogues_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            viewModel.SelectedTextGroup = ((ListBox)sender).SelectedItem as Dialogue;
        }

        private void Dialogues_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
