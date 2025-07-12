using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using DQB2TextEditor.Linkdata;
using DQB2TextEditor.Proccessing;

namespace DQB2TextEditor.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class SelectLinkdata : Window
    {
        //No need for double bind I will not update this.
        private SLViewModel ViewModel;
        public SelectLinkdata()
        {
            ViewModel = new SLViewModel();
            DataContext = ViewModel;
            InitializeComponent();
            LINKDATAReader.initLINKDATAReader();

        }
        private void LinkdataPathUpdate(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "LINKDATA|LINKDATA.IDX";
            if (dlg.ShowDialog() == false) return;

            ViewModel.LinkdataPath = dlg.FileName;
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {

            TextEditorWindow window = new TextEditorWindow(ViewModel.CreateLINKDATA());
            window.Show();
            this.Close();
        }
    }

    public class SLViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private string _linkdataPath = string.Empty;
        private uint _size = 0;
        private string _versionName = string.Empty;
        private bool linkdataerror = false;
        private bool versionerror = false;
        public Brush LinkdataError => linkdataerror ? (System.Windows.Media.Brush)System.Windows.Application.Current.Resources["MediumOrangeBrush"] : Brushes.White; //Change
        public Brush VersionError => versionerror ? (System.Windows.Media.Brush)System.Windows.Application.Current.Resources["MediumOrangeBrush"] : Brushes.White; //Change
        public Brush VersionErrorSize => versionerror ? Brushes.Gold : Brushes.White; //Change
        public bool ConfirmEnabled => !string.IsNullOrEmpty(LinkdataPath) && !linkdataerror && !versionerror;
        public string LinkdataPath
        {
            get { return _linkdataPath; }
            set
            {
                if (_linkdataPath != value)
                {
                    _linkdataPath = value;
                    OnPropertyChanged(nameof(LinkdataPath));
                    try
                    {
                        var a = LINKDATAReader.GetVersion(_linkdataPath);
                        VersionName = a.Key;
                        _size = a.Value;
                        linkdataerror = false;
                        versionerror = false;
                    }
                    catch (KeyNotFoundException ex)
                    {
                        VersionName = "Unknown Version";
                        _size = uint.Parse(ex.Message);
                        versionerror = true;
                        linkdataerror = false;

                    }
                    catch (Exception ex)
                    {
                        linkdataerror = true;
                        versionerror = false;
                        VersionName = "---";
                        _size = 0;
                    }
                    OnPropertyChanged(nameof(LinkdataError));
                    OnPropertyChanged(nameof(VersionError));
                    OnPropertyChanged(nameof(VersionErrorSize));
                    OnPropertyChanged(nameof(ConfirmEnabled));
                    OnPropertyChanged(nameof(Size));
                }

            }
        }
        public string Size
        {
            get { return _size == 0 ? string.Empty : _size.ToString(); }
        }
        public string VersionName
        {
            get { return _versionName; }
            set
            {
                if (_versionName != value)
                {
                    _versionName = value;
                    OnPropertyChanged(nameof(VersionName));
                }
            }
        }

        public LINKDATA CreateLINKDATA()
        {
            return new LINKDATA(LinkdataPath, _size);
        }
    }
}
