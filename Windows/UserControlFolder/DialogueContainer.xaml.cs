using DQB2TextEditor.InfoReading;
using DQB2TextEditor.Linkdata.LineEntry;
using DQB2TextEditor.Windows.Panel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using System.Xml.Linq;

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
            }
        }

        public ObservableCollection<string> Arguments
        {
            get
            {
                ObservableCollection<string> args = new ObservableCollection<string>();
                for (int i = 0; i < FlowDataEntry.argumentCount; i++)
                {
                    if(FlowDataEntry.GetArgumentName(i) != null)
                        args.Add(i + " | " + FlowDataEntry.GetArgumentName(i) + "\n     "+ FlowDataEntry.GetArgument(i).Item1.ToString());
                }
                return args;
               
            }
        }
        public String CommandName => FlowDataEntry.command + " | " + FlowDataEntry.GetCommandName();

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
        private Visibility _nameVisible = Visibility.Hidden;
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

        public string Line
        {
            get {
                if (ViewModel.Asia && CharName != null)
                {
                    return CharName + "「" + FlowDataEntry.Line.Replace("<br>", Environment.NewLine + "    ") ;
                }
                else
                {
                    return FlowDataEntry.Line;
                }
            }
        }


        private void RunCheck(object sender, RoutedEventArgs e)
        {
           
               
            if (FlowDataEntry.Line != null && FlowDataEntry.command == 118){
                NameVisible = Visibility.Hidden;
                TextBoxBG.Background = Brushes.Transparent;
                TextBoxName.Background = Brushes.Transparent;
                TextBoxBG.BorderBrush = Brushes.Transparent;
                TextBoxName.BorderBrush = Brushes.Transparent;
                TextBoxLine.TextAlignment = TextAlignment.Center;
            }
            else
            {
                if (FlowDataEntry.Line != null && FlowDataEntry.Line.Contains("<show("))
                {
                    CharName = "<$cdef(73)>" + Regex.Match(FlowDataEntry.Line, @"<show\((.*?)\)>").Groups[1].Value + "</color>";
                }
                else
                    if (FlowDataEntry.GetArgumentName(0) != null && FlowDataEntry.GetArgument(0).Item2 == typeof(Character))
                    {
                        if ((int)FlowDataEntry.GetArgument(0).Item1 == 0) 
                            CharName = null;
                        else
                            CharName = InformationReading.GetCharNames((ushort)(int)FlowDataEntry.GetArgument(0).Item1, ViewModel._currentLanguage);
                    }

                //NAME ASIA
                if (ViewModel.Asia)
                {
                    if (FlowDataEntry.Line != null && !String.IsNullOrEmpty(FlowDataEntry.Line.Trim()) && !FlowDataEntry.Line.Trim().Equals("\0"))
                    {
                        NameVisible = Visibility.Hidden;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Line)));
                    }
                    else
                    {
                        NameVisible = Visibility.Hidden;
                        LineVisible = Visibility.Hidden;
                    }
                    
                }
                else //NAME EU
                {
                    if (FlowDataEntry.Line != null && !String.IsNullOrEmpty(FlowDataEntry.Line.Trim()) && !FlowDataEntry.Line.Trim().Equals("\0"))
                    {
                        NameVisible = Visibility.Visible;
                    }
                    else
                    {
                        NameVisible = Visibility.Hidden;
                        LineVisible = Visibility.Hidden;
                    }
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
}
