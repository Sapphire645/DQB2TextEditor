using DQB2TextEditor.InfoReading;
using DQB2TextEditor.Linkdata.LineEntry;
using DQB2TextEditor.Windows.Panel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace DQB2TextEditor.Windows.UserControlFolder
{
    /// <summary>
    /// Interaction logic for DialogueContainer.xaml
    /// </summary>
    public partial class DialogueContainer : UserControl, INotifyPropertyChanged
    {
        public DialogueContainer()
        {
            InitializeComponent();
        }
       
        public static readonly DependencyProperty FlowDataEntryProperty =
        DependencyProperty.Register(nameof(FlowDataEntry), typeof(FlowDataLine), typeof(DialogueContainer), null);

        public event PropertyChangedEventHandler? PropertyChanged;
        public FlowDataLine FlowDataEntry
        {
            get => (FlowDataLine)GetValue(FlowDataEntryProperty);
            set
            {
                SetValue(FlowDataEntryProperty, value);
                //Change
                if (value.GetArgumentName(0).Equals("char")){
                    CharName = InformationReading.GetCharNames((ushort)(int)value.GetArgument(0).Item1, ViewModel._currentLanguage);
                }
                if(value.Line != null && value.Line != "")
                {
                    NameVisible = Visibility.Visible;
                }
                else
                {
                    NameVisible = Visibility.Collapsed;
                }


            }
        }

        public ObservableCollection<string> Arguments
        {
            get
            {
                ObservableCollection<string> args = new ObservableCollection<string>();
                for (int i = 0; i < FlowDataEntry.argumentCount; i++)
                {
                    args.Add(FlowDataEntry.GetArgumentName(i) + "\n"+ FlowDataEntry.GetArgument(i).Item1.ToString());
                }
                return args;
                    
            }
        }
        public String CommandName => FlowDataEntry.GetCommandName();

        public bool unkonownFlag => FlowDataEntry.unkonownFlag;

        private Visibility _lineVisible = Visibility.Visible;
        public Visibility LineVisible
        {
            get => _lineVisible;
            set
            {
                _lineVisible = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LineVisible)));
            }
        }
        private Visibility _nameVisible = Visibility.Collapsed;
        public Visibility NameVisible
        {
            get => _nameVisible;
            set
            {
                _nameVisible = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NameVisible)));
            }
        }
        private string _charName = "*";
        public string CharName
        {
            get => _charName;
            set
            {
                _charName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CharName)));
            }
        }



        private void RunCheck(object sender, RoutedEventArgs e)
        {
            if (FlowDataEntry.GetArgumentName(0) != null && FlowDataEntry.GetArgumentName(0).Equals("char"))
            {
                CharName = InformationReading.GetCharNames((ushort)(int)FlowDataEntry.GetArgument(0).Item1, ViewModel._currentLanguage);
            }
            if (FlowDataEntry.Line != null && !String.IsNullOrEmpty(FlowDataEntry.Line.Trim()) && !FlowDataEntry.Line.Trim().Equals("\0"))
            {
                NameVisible = Visibility.Visible;
            }
            else
            {
                NameVisible = Visibility.Collapsed;
                LineVisible = Visibility.Collapsed;
            }
            if (FlowDataEntry.Line != null && FlowDataEntry.Line.Contains("<off>")) //Turn off bg
            {
                TextBoxBG.Background = Brushes.Transparent;
                TextBoxName.Background = Brushes.Transparent;
                TextBoxBG.BorderBrush = Brushes.Transparent;
                TextBoxName.BorderBrush = Brushes.Transparent;
            }
            else
            {

            }//Turn off bg
            
        }
    }
}
