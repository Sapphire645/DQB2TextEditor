using DQB2TextEditor.Windows;
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
        private const string CODE_PATH = "Info/code.txt";

        private static Brush[] colourBrushes;
        public static Brush[] ColourBrushes { get { if (colourBrushes == null) ReadPreviewData(COLOR_PATH); return colourBrushes; } }

        private static Dictionary<ushort,CommandInfo> commands;
        public static Dictionary<ushort, CommandInfo> Commands { get { if (commands == null) ReadCommands(CODE_PATH); return commands; } }

        private static Dictionary<ushort, string[]> nameTable = null;

        public static string GetCharNames(ushort id, int lang)
        {
            if (nameTable == null)
                BuildNameTable();
            if (nameTable.ContainsKey(id))
                return nameTable[id][lang];
            return "*";
        }

        private static void BuildNameTable()
        {
            nameTable = new Dictionary<ushort, string[]>();
            var res = vModel.GetNPCFiles();
            ushort j;
            for (int i = 0; i < res.Item1.Count; i++)
            {
                switch (res.Item2[i])
                {
                    case true: //Asian lang
                        for (ushort index = 0; index < res.Item1[i].Count; index += 1)
                        {
                            if (!nameTable.ContainsKey(index))
                                nameTable.Add(index, new string[res.Item1.Count]);
                            nameTable[index][i] = res.Item1[i][index];
                        }
                        break;
                    case false:
                        j = 0;
                        for (int index = 11; index < res.Item1[i].Count; index += 12)
                        {
                            if (!nameTable.ContainsKey(j))
                                nameTable.Add(j, new string[res.Item1.Count]);
                            nameTable[j][i] = res.Item1[i][index];
                            j++;
                        }
                        break;
                }
            }
        }

        public static ObtainData vModel;




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

        private static void ReadCommands(string filename)
        {
            if (!System.IO.File.Exists(filename)) return;
            commands = new Dictionary<ushort, CommandInfo>();
            String[] lines = System.IO.File.ReadAllLines(filename);
            var Current = -1;
           
            foreach (String line in lines)
            {
                CommandInfo Command = new CommandInfo(line.Trim());
                Commands.Add(Command.Command, Command);
            }
        }
    }
}
