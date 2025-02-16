using Microsoft.Win32;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace DQB2TextEditor.code
{
    /// <summary>
    /// Interaction logic for DumpCode.xaml
    /// </summary>
    public partial class DumpCode : Window
    {
        private DumpViewModel DumpViewModel;
        public DumpCode(string linkdataPath, VersionInformation versionInfo)
        {
            DumpViewModel = new DumpViewModel(linkdataPath, versionInfo);
            this.DataContext = DumpViewModel;
            InitializeComponent();
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
        private void LinkdataTwoClick(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "LINKDATA|LINKDATA.IDX";
            if (dlg.ShowDialog() == false) return;

            DumpViewModel.LinkdataIDXtwo = dlg.FileName;
        }
        private void LinkdataOneClick(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "LINKDATA|LINKDATA.IDX";
            if (dlg.ShowDialog() == false) return;

            DumpViewModel.LinkdataIDXone = dlg.FileName;
        }

        private void DumpCommand(object sender, RoutedEventArgs e)
        {
            string command = (sender as Button).Tag.ToString();
            string name ="";
            switch (command)
            {
                case "0":
                    name = "Full" + DumpViewModel.dataOneShow.VersionFile;
                    break;
                case "1":
                    name = "Text" + DumpViewModel.dataOneShow.VersionFile;
                    break;
                case "2":
                    name = "Full" + DumpViewModel.dataOneShow.VersionFile + DumpViewModel.dataTwoShow.VersionFile;
                    break;
                case "3":
                    name = "Text" + DumpViewModel.dataOneShow.VersionFile + DumpViewModel.dataTwoShow.VersionFile;
                    break;
            }
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "*.txt|*.txt",
                FileName = "Dump" + name
            };
            if (saveFileDialog.ShowDialog() == false) return;
            ProgressBar.Value = 0;
            ProgressLabel.Text = "0/" + ProgressBar.Maximum;
            ProgressBarGrid.Visibility = Visibility.Visible;
            DumpViewModel.Dump(saveFileDialog.FileName, command, UpdateDumpExport, FinishedDump);

        }
        private void FinishedDump(object sender, RunWorkerCompletedEventArgs e)
        {
            ProgressBarGrid.Visibility = Visibility.Hidden;
        }
        private void UpdateDumpExport(object sender, ProgressChangedEventArgs e)
        {
            ProgressLabel.Text = e.UserState + "/" + ProgressBar.Maximum;
            ProgressBar.Value = (int)e.UserState;

            int Value = (int)e.UserState;
            byte R = (byte)((Value / 16) > 255 ? 255 - (Value / 16) : 255);
            byte G = (byte)((Value / 16) < 255 ? (Value / 16) : 255);

            ProgressBar.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(R,G,0));
        }
    }
}
