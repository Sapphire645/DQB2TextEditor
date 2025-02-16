using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
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

namespace DQB2TextEditor.code
{
    /// <summary>
    /// Interaction logic for DialogueEntry.xaml
    /// </summary>
    public partial class DialogueEntry : System.Windows.Controls.UserControl
    {
        public ObservableProperty<Entry> Entry { get; set; } = new ObservableProperty<Entry>();
        public Entry EntryEdit { get { if(loaded) Entry.NotifyValue(); return Entry.Value; } set { Entry.Value = value; } }

        public bool mPanel = false;
        private bool loaded = false;

        public WeakReference<Panel> panelReference;
        public bool Preview
        {
            get { return false; }
            set
            {
                if (value && !Entry.Value.Line.Equals("\0"))
                {
                    TextPreview.Visibility = Visibility.Visible;
                    TextNormal.Visibility = Visibility.Collapsed;
                    TextPreviewBox.Document.Blocks.Clear();
                    TextPreviewBox.Document.Blocks.Add(Entry.Value.LineProcess());
                }
                else
                {
                    TextPreview.Visibility = Visibility.Collapsed;
                    TextNormal.Visibility = Visibility.Visible;
                }
            }
        }
        public ObservableProperty<Visibility> VisibilityPanel { get; set; } = new ObservableProperty<Visibility> { Value = Visibility.Collapsed };
        public bool Panel { get
            {
                return mPanel;
            }
            set
            {
                if (Entry.Value.Arguments == null || VersionInformation.Encrypted)
                {
                    mPanel = false;
                    return;
                }

                mPanel = !mPanel;
                if (mPanel)
                {
                    VisibilityPanel.Value = Visibility.Visible;
                    
                    if (panelReference == null || !panelReference.TryGetTarget(out var panel) || panel == null)
                    {
                        panel = new code.Panel(this);
                        panelReference = new WeakReference<Panel>(panel);
                    }
                    StackAddPanel.Children.Add(panel);
                }
                else
                {
                    VisibilityPanel.Value = Visibility.Collapsed;
                    StackAddPanel.Children.Clear();
                }
            }
        }
        public DialogueEntry(Entry entry)
        {
            Entry.Value = entry;
            InitializeComponent();
            DataContext = this;
            loaded = true;
        }

        public void UpdateNewText(Entry entry)
        {
            Entry.Value = entry;
            panelReference = null;
            if (mPanel)
                Panel = false;
        }

        private void AddDeleteEntry(object sender, MouseButtonEventArgs e)
        {
            if (Entry.Value.Arguments == null)
            {
                //add
                Entry.Value.Arguments = new int[11];
                for(int i = 0; i < 11; i++)
                {
                    Entry.Value.Arguments[i] = -1;
                }
                Entry.Value.Line = "...";
                Panel = true;
            }

            else
            {
                Panel = false;
                Entry.Value.Arguments = null;
                Entry.Value.Line = "";
            }
            Entry.Value.Command = 0;
            EntryEdit.LastArgument = true;
            var currentDataContext = this.DataContext;
            this.DataContext = null;
            this.DataContext = currentDataContext;
        }

        private void ShowPlus(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Plus.Visibility = Visibility.Visible;
        }
        private void HidePlus(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Plus.Visibility = Visibility.Collapsed;
        }
    }
}
