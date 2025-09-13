using DQB2TextEditor.Windows.UserControlFolder;
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

namespace DQB2TextEditor.Windows.Panel
{
    /// <summary>
    /// Interaction logic for TextEntry.xaml
    /// </summary>
    public partial class TextEntry : UserControl
    {
        public TextEntry()
        {
            InitializeComponent();

        }

        public void AddLine(TextContainer textContainer)
        {

        }
        private void Line_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ListBox)sender).SelectedItem == null) 
                (DataContext as ViewModel).EditingLine = null;
            else
                (DataContext as ViewModel).EditingLine = ((ListBox)sender).SelectedItem.ToString();
        }

        private TextContainer _lastHoveredItem;

        private void ListBox_MouseMove(object sender, MouseEventArgs e)
        {
            var listBox = (ListBox)sender;
            var element = listBox.InputHitTest(e.GetPosition(listBox)) as DependencyObject;
            var container = ItemsControl.ContainerFromElement(listBox, element) as ListBoxItem;
            if (container != null)
            {
                var item = FindVisualChild<TextContainer>(container);
                if (!Equals(item, _lastHoveredItem))
                {
                    OnHoveredItemChanged(item);
                }
            }
            else if (_lastHoveredItem != null)
            {
                OnHoveredItemChanged(null);
            }
        }
        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T found)
                    return found;

                var result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }
        private void OnHoveredItemChanged(TextContainer newItem)
        {
            if(_lastHoveredItem != null) _lastHoveredItem.removeAddButton();
            if (newItem != null) newItem.displayAddButton(this);
            _lastHoveredItem = newItem;
        }

        private void SaveLine(object sender, RoutedEventArgs e)
        {
            if (LinesList.SelectedItem != null)
                (DataContext as ViewModel).TextLinesEdit[LinesList.SelectedIndex] = (DataContext as ViewModel).EditingLine;


        }
    }
}
