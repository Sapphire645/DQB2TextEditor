using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Resources;

namespace DQB2TextEditor.InfoReading
{
    public static class InformationReading
    {
        private const string COLOR_PATH = "Info/colorcodes.txt";

        private static Brush[] colourBrushes;
        public static Brush[] ColourBrushes { get { if (colourBrushes == null) ReadPreviewData(COLOR_PATH); return colourBrushes; } }

        private static void ReadPreviewData(string filename)
        {
            String[] lines = ReadEmbeddedResource(filename).Split("\n");
            colourBrushes = new Brush[lines.Length - 1];
            var Current = 0;
            BrushConverter brushConverter = new BrushConverter();
            foreach (String line in lines)
            {
                if (line[0] == '#') continue;
                var col = "#" + line.Trim();
                colourBrushes[Current] = (System.Windows.Media.Brush)brushConverter.ConvertFromString(col);
                Current++;
            }
        }

        //still whatever
        private static string ReadEmbeddedResource(string path)
        {
            Uri resourceUri = new Uri(path, UriKind.Relative);
            StreamResourceInfo resourceInfo = Application.GetResourceStream(resourceUri);

            if (resourceInfo != null)
            {
                using (StreamReader reader = new StreamReader(resourceInfo.Stream))
                {
                    return reader.ReadToEnd();
                }
            }
            return string.Empty;
        }
    }
}
