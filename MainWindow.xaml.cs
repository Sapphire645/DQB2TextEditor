using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DQB2TextEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ViewModel viewModel;
        public MainWindow()
        {
            viewModel = new ViewModel();
            this.DataContext = viewModel;
            InitializeComponent();
        }

        private void Continue(object sender, RoutedEventArgs e)
        {
            String path = viewModel.LinkdataPath.Value;
            if (!System.IO.File.Exists(path)) return;
            path = path.Substring(0, path.Length - 3) + "BIN";
            if (!System.IO.File.Exists(path)) return;

            if (viewModel.VersionInfo.VersionFile == null) return;

            if (viewModel.VersionInfo.LINKDATASize != viewModel.LinkdataEntries.Value) return;

            CutsceneBrowser Window = new CutsceneBrowser(viewModel);
            Window.Show();

            this.Close();
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
                if(this.WindowState == WindowState.Maximized)
                {
                    this.WindowState = WindowState.Normal;
                    MaximizeButton.Content = "⬜";
                }
                this.DragMove();
            }
        }

        private void Update(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if(viewModel == null) return;
            viewModel.VersionInfo.Update();

            if (viewModel.VersionInfo.LINKDATASize != viewModel.LinkdataEntries.Value) viewModel.ConfirmEnabled.Value = false;
            else viewModel.ConfirmEnabled.Value = true;

            var refresh = DataContext;
            DataContext = null;
            DataContext = refresh;
        }
    }
}
