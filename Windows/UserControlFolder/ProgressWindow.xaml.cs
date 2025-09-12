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

namespace DQB2TextEditor.Windows.UserControlFolder
{
    /// <summary>
    /// Interaction logic for ProgressBar.xaml
    /// </summary>
    public partial class ProgressWindow : Window
    {
        public ProgressWindow(string information, string data, uint top)
        {
            
            InitializeComponent();
            tname.Text = information;
            searching.Text = data;
            Bar.Maximum = top;
            progress.Text = "0/" + top.ToString();
        }
    }
}
