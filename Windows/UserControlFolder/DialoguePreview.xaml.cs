using DQB2TextEditor.InfoReading;
using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace DQB2TextEditor.Windows.UserControlFolder
{
    /// <summary>
    /// Interaction logic for DialoguePreview.xaml
    /// </summary>
    public partial class DialoguePreview : UserControl
    {
        public DialoguePreview()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty DisplayTextProperty =
        DependencyProperty.Register(nameof(DisplayText), typeof(string), typeof(DialoguePreview), new PropertyMetadata(""));

        public string DisplayText
        {
            get => (string)GetValue(DisplayTextProperty);
            set => SetValue(DisplayTextProperty, value);
        }
    }


    public class HighlightWordConverter : IValueConverter
    {
        public string WordToHighlight { get; set; }
        public Brush HighlightColor { get; set; } = Brushes.Red;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value as string;
            var word = WordToHighlight;

            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(word))
                return new Inline[] { new Run(text ?? "") };

            var inlines = new List<Inline>();
            var parts = text.Split(new[] { word }, StringSplitOptions.None);

            for (int i = 0; i < parts.Length; i++)
            {
                inlines.Add(new Run(parts[i]));

                if (i < parts.Length - 1)
                {
                    inlines.Add(new Run(word)
                    {
                        Foreground = HighlightColor,
                        FontWeight = FontWeights.Bold
                    });
                }
            }

            return inlines;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public static class TextBlockExtensions
    {
        public static readonly DependencyProperty HighlightedTextProperty =
            DependencyProperty.RegisterAttached(
                "HighlightedText",
                typeof(string),
                typeof(TextBlockExtensions),
                new PropertyMetadata(string.Empty, OnHighlightedTextChanged));

        public static string GetHighlightedText(TextBlock textBlock)
            => (string)textBlock.GetValue(HighlightedTextProperty);

        public static void SetHighlightedText(TextBlock textBlock, string value)
            => textBlock.SetValue(HighlightedTextProperty, value);

        private static void OnHighlightedTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextBlock textBlock || e.NewValue is not string newText)
                return;

            textBlock.Inlines.Clear();
            var LineProcessed = newText.Replace("<6>", "‛");
            LineProcessed = LineProcessed.Replace("<9>", "’");
            LineProcessed = LineProcessed.Replace("<66>", "“");
            LineProcessed = LineProcessed.Replace("<99>", "”");
            LineProcessed = LineProcessed.Replace("<1>", "`");
            LineProcessed = LineProcessed.Replace("<key>", "");
            LineProcessed = LineProcessed.Replace("<scron>", "");
            LineProcessed = LineProcessed.Replace("<scroff>", "");
            LineProcessed = LineProcessed.Replace("<off>", "");
            LineProcessed = LineProcessed.Replace("<-->", "─");
            LineProcessed = LineProcessed.Replace("<br>", Environment.NewLine);
            LineProcessed = LineProcessed.Replace("<--->", "⎯⎯ ");
            LineProcessed = LineProcessed.Replace("<note>", "♩");
            LineProcessed = LineProcessed.Replace("<pname>", ViewModel.PlayerName);
            LineProcessed = Regex.Replace(LineProcessed, @"(?<=<cap>)[a-zA-Z]", match => match.Value.ToUpper());
            LineProcessed = LineProcessed.Replace("<cap>", "");
            LineProcessed = Regex.Replace(LineProcessed, @"<allcap>(.*?)</allcap>", match => match.Groups[1].Value.ToUpper()); //allcap
            LineProcessed = Regex.Replace(LineProcessed, @"<morf\((.*?),(.*?)\)>", match => match.Groups[ViewModel.Gender ? 2 : 1].Value);

            var ColourLines = Regex.Split(LineProcessed, $"(?={Regex.Escape(@"</color>") + "|" + Regex.Escape(@"<$cdef(")})");

            foreach (var ColourText in ColourLines)
            {
                if (ColourText.StartsWith("<$cdef(") && int.TryParse(Regex.Match(newText, @"<\$cdef\((\d+)\)>").Groups[1].Value, out var ColourNumber))
                {
                    var Line = Regex.Replace(ColourText, @"<\$cdef\((.*?)\)>", "");
                    textBlock.Inlines.Add(new Run(Line)
                    {
                        Foreground = InformationReading.ColourBrushes[ColourNumber],
                    });
                    //ProcessLineJp(Paragraph, Line, (System.Windows.Media.Brush)brushConverter.ConvertFromString(VersionInformation.ColourPreview[ColourNumber]));
                }
                else
                {
                    var Line = ColourText.Replace("</color>", "");
                    textBlock.Inlines.Add(new Run(Line));
                }
            }
        }
    }
}
