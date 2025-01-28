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

namespace DQB2TextEditor.code
{
    /// <summary>
    /// Interaction logic for TextValues.xaml
    /// </summary>
    public partial class TextValues : Window
    {
        public TextValues()
        {
            InitializeComponent();
            DataContext = this;
            TextBoxName.Text = VersionInformation.PlayerNameDefault;
            MaleG.IsChecked = !VersionInformation.PlayerGender;
            FemaleG.IsChecked = VersionInformation.PlayerGender;

        }
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            // Toggle between Maximize and Restore
            if (this.WindowState == WindowState.Normal)
            {
                this.WindowState = WindowState.Maximized;
                MaximizeButton.Content = "⧉";
            }
            else
            {
                this.WindowState = WindowState.Normal;
                MaximizeButton.Content = "⬜";
            }
        }
        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (this.WindowState == WindowState.Maximized)
                {
                    this.WindowState = WindowState.Normal;
                    MaximizeButton.Content = "⬜";
                }
                this.DragMove();
            }
        }
        private void test(object sender, RoutedEventArgs e)
        {
            if (((System.Windows.Controls.RadioButton)sender).Name.Equals("MaleG"))
                VersionInformation.PlayerGender = false;
            else
                VersionInformation.PlayerGender = true;
            MaleG.IsChecked = !VersionInformation.PlayerGender;
            FemaleG.IsChecked = VersionInformation.PlayerGender;
            TextBoxName.Text = VersionInformation.PlayerNameDefault;
        }

        private void OutText(object sender, RoutedEventArgs e)
        {
            TextBoxName.Text = VersionInformation.PlayerName;
        }

        private void InText(object sender, RoutedEventArgs e)
        {
            TextBoxName.Text = VersionInformation.PlayerNameDefault;
            
        }

        private void TextChange(object sender, TextChangedEventArgs e)
        {
            if(!TextBoxName.Text.Equals(VersionInformation.PlayerNameDefault))
                VersionInformation.PlayerName = TextBoxName.Text;
        }
    }
}
