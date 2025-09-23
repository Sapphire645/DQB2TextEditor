using DQB2TextEditor.Linkdata;
using DQB2TextEditor.Windows.UserControlFolder;
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
    /// MISSING
    /// - EXPORT AND IMPORT
    /// - READ FLOWDATA
    /// - NORMAL TEXT 
    /// SAVING INTO THE LINKDATA.
    /// </summary>
    public partial class TextEditorWindow : Window
    {
        private ViewModel viewModel;
        public TextEditorWindow(SLViewModel SLVM)
        {
            viewModel = new ViewModel(SLVM, this);
            DataContext = viewModel;
            InitializeComponent();

        }

        private void Dialogues_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            viewModel.SelectedTextGroup = ((ListBox)sender).SelectedItem as Dialogue;
        }
        private void MenuTexts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            viewModel.SelectedTextGroup = ((ListBox)sender).SelectedItem as TextGroup;
        }

        private void Dialogues_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ExportMenu_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            button.ContextMenu.PlacementTarget = button;
            button.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            button.ContextMenu.IsOpen = true;
        }

        private void Preview_Click(object sender, RoutedEventArgs e)
        {
            viewModel.UpdatePreviewText();
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            viewModel.TextFilter();
        }
        private void Search_Click_General(object sender, RoutedEventArgs e)
        {
            viewModel.TextFilterGeneral();
        }

        private void SizeChange(object sender, SizeChangedEventArgs e)
        {
            viewModel.SizeChange();

        }

        private void Preview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(((ListBox)sender).SelectedItem != null)
                Clipboard.SetText(((ListBox)sender).SelectedItem.ToString()); 
        }

        private void Edit_Selected_Click(object sender, RoutedEventArgs e)
        {
            viewModel.SelectedToEdit();
        }

        private void Save_Edited_Click(object sender, RoutedEventArgs e)
        {
            viewModel.EditToSelected();
        }


    }
}
