using DQB2TextEditor.Windows.Panel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DQB2TextEditor.Windows.UserControlFolder
{
    /// <summary>
    /// Interaction logic for TextContainer.xaml
    /// </summary>
    public partial class TextContainer : UserControl
    {

        private TextEntry parent; //me when i cheat
        public TextContainer()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty DisplayTextProperty =
        DependencyProperty.Register(nameof(DisplayText), typeof(string), typeof(TextContainer), new PropertyMetadata(""));

        public string DisplayText
        {
            get => (string)GetValue(DisplayTextProperty);
            set
            {
                SetValue(DisplayTextProperty, value);
            }
        }

        private void RunCheck(object sender, RoutedEventArgs e)
        {
            if (DisplayText.Contains("<off>")) //Turn off bg
            {
                TextBoxBG.Background = Brushes.Transparent;
                TextBoxBG.BorderBrush = Brushes.Transparent;
            }
            else
            {

            }//Turn off bg
        }

        public void displayAddButton(TextEntry par)
        {
            parent = par;
            AddButtonName.Visibility = Visibility.Visible;
        }

        public void removeAddButton()
        {
            AddButtonName.Visibility = Visibility.Collapsed;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            parent.AddLine(this);
        }
    }
}
