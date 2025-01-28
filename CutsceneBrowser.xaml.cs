using DQB2TextEditor.code;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace DQB2TextEditor
{
    /// <summary>
    /// Interaction logic for CutcseneBrowser.xaml
    /// </summary>
    public partial class CutsceneBrowser : Window
    {
        private bool popupOpen=false;

        private ViewModel ViewModel;
        public CutsceneBrowser(ViewModel viewModel)
        {
            ViewModel = viewModel;
            viewModel.ContextChangeOne();
            this.DataContext = viewModel;
            this.SizeChanged += OnWindowSizeChanged;

            InitializeComponent();
            viewModel.ContextChangeTwo(ListBox, RawText, ListBoxCutscene, Saving, this);
            OPCodeText.Text = viewModel.VersionInfo.getOpcode();
        }
        protected void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var a = this.AeeDok.ActualWidth;
            this.ListBox.Height = (AeeDok.ActualHeight-60) * ListBox.Width / (AeeDok.ActualWidth - 20);
            this.ListBoxCutscene.Height = (AeeDok.ActualHeight - 60) * ListBox.Width / (AeeDok.ActualWidth - 20);
            RawText.Height = (AeeDok.ActualHeight - 60) * ListBox.Width / (AeeDok.ActualWidth - 20);
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
        private object oldLan = null;
        private void UpdateIDXIndex(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if(oldLan != (sender as System.Windows.Controls.ComboBox).SelectedItem && oldLan != null)
            {
                oldLan = (sender as System.Windows.Controls.ComboBox).SelectedItem;
                var refresh = DataContext;
                DataContext = null;
                DataContext = refresh;
            }
            oldLan = (sender as System.Windows.Controls.ComboBox).SelectedItem;
        }

        private void CommandUpdateSelectedDialogue(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ViewModel.UpdateSelectedDialogue((Dialogue)((System.Windows.Controls.ListBox)sender).SelectedItem);
        }

        private void EditValues(object sender, RoutedEventArgs e)
        {
            TextValues Window = new TextValues();
            ViewPopup.IsOpen = false;
            OpenPopupButton.BorderBrush = System.Windows.Media.Brushes.Transparent;
            Window.Show();
        }

        private void OpenPopup(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!popupOpen) return;
            string tag = (sender as System.Windows.Controls.Button).Tag.ToString();
            switch (tag)
            {
                case "v":
                    ViewPopup.IsOpen = true;
                    OpenPopupButton.BorderBrush = (System.Windows.Media.Brush)System.Windows.Application.Current.Resources["DarkSelectedBrush"];
                    break;
                case "e":
                    ViewPopupExport.IsOpen = true;
                    OpenPopupButtonExport.BorderBrush = (System.Windows.Media.Brush)System.Windows.Application.Current.Resources["DarkSelectedBrush"];
                    break;
                case "i":
                    ViewPopupImport.IsOpen = true;
                    OpenPopupButtonImport.BorderBrush = (System.Windows.Media.Brush)System.Windows.Application.Current.Resources["DarkSelectedBrush"];
                    break;
            }
        }

        private void ClosePopup(object sender, System.Windows.Input.MouseEventArgs e)
        {
            string tag;
            if(sender is Border)
            {
                tag = (sender as Border).Tag.ToString();
            }
            else
            {
                tag = (sender as System.Windows.Controls.Button).Tag.ToString();
            }
            switch (tag)
            {
                case "v":
                    if (ViewPopup.IsMouseOver == false)
                    {
                        ViewPopup.IsOpen = false;
                        OpenPopupButton.BorderBrush = System.Windows.Media.Brushes.Transparent;
                    }
                    break;
                case "e":
                    if (ViewPopupExport.IsMouseOver == false)
                    {
                        ViewPopupExport.IsOpen = false;
                        OpenPopupButtonExport.BorderBrush = System.Windows.Media.Brushes.Transparent;
                    }
                    break;
                case "i":
                    if (ViewPopupImport.IsMouseOver == false)
                    {
                        ViewPopupImport.IsOpen = false;
                        OpenPopupButtonImport.BorderBrush = System.Windows.Media.Brushes.Transparent;
                    }
                    break;
            }
        }

        private void PopupClick(object sender, RoutedEventArgs e)
        {
            if (popupOpen)
            {
                ViewPopupExport.IsOpen = false;
                OpenPopupButtonExport.BorderBrush = System.Windows.Media.Brushes.Transparent;
                ViewPopupImport.IsOpen = false;
                OpenPopupButtonImport.BorderBrush = System.Windows.Media.Brushes.Transparent;
                ViewPopup.IsOpen = false;
                OpenPopupButton.BorderBrush = System.Windows.Media.Brushes.Transparent;
            }
            else
            {
                string tag = (sender as System.Windows.Controls.Button).Tag.ToString();
                switch (tag)
                {
                    case "v":
                        ViewPopup.IsOpen = true;
                        OpenPopupButton.BorderBrush = (System.Windows.Media.Brush)System.Windows.Application.Current.Resources["DarkSelectedBrush"];
                        break;
                    case "e":
                        ViewPopupExport.IsOpen = true;
                        OpenPopupButtonExport.BorderBrush = (System.Windows.Media.Brush)System.Windows.Application.Current.Resources["DarkSelectedBrush"];
                        break;
                    case "i":
                        ViewPopupImport.IsOpen = true;
                        OpenPopupButtonImport.BorderBrush = (System.Windows.Media.Brush)System.Windows.Application.Current.Resources["DarkSelectedBrush"];
                        break;
                }
            }
            popupOpen = !popupOpen;
        }

        private void AboutWindow(object sender, RoutedEventArgs e)
        {
            About Window = new About();
            Window.Show();
        }
        private string oldFilter = null;
        private void UpdateFilter(object sender, TextChangedEventArgs e)
        {
            ViewModel.FilterList((sender as System.Windows.Controls.TextBox).Text);
            if(oldFilter != (sender as System.Windows.Controls.TextBox).Text)
            {
                oldFilter = (sender as System.Windows.Controls.TextBox).Text;
                var refresh = DataContext;
                DataContext = null;
                DataContext = refresh;
            }
            oldFilter = (sender as System.Windows.Controls.TextBox).Text;
        }
    }
}
