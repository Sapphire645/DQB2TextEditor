using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace DQB2TextEditor.Proccessing
{
    public static class LINKDATAReader
    {
        static readonly string InfoPath = "Info/versions/";
        public static Dictionary<uint, string> Version;

        public static void initLINKDATAReader()
        {
            Version = new Dictionary<uint, string>();
            //Get all versions.
            foreach (String version in Directory.GetFiles(InfoPath))
            {
                foreach (var Line in System.IO.File.ReadAllLines(version))
                {
                    if (Line.StartsWith("-SIZE"))
                    {
                        uint.TryParse(Line.Split('\t').Last(), out var LS);
                        Version.Add(LS, version);
                    }
                }
            }
        }
        public static KeyValuePair<string, uint> GetVersion(string LinkdataPath)
        {
            if (!File.Exists(LinkdataPath)) throw new Exception("Not Valid Linkdata.");

            FileInfo fileInfo = new FileInfo(LinkdataPath);
            var LinkdataSize = (uint)(fileInfo.Length / 32);

            if (Version.TryGetValue(LinkdataSize, out var versionName))
            {
                return new KeyValuePair<string, uint>(versionName.Split('/').Last().Replace(".txt",""), LinkdataSize);
            }
            else
            {
                throw new KeyNotFoundException(LinkdataSize.ToString());
            }
        }
        public static string GetPath(uint LinkdataSize)
        {
            if (Version.TryGetValue(LinkdataSize, out var versionName))
            {
                return versionName;
            }
            else
            {
               MessageBox.Show($"???????", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
               throw new KeyNotFoundException(LinkdataSize.ToString()); //die
            }
        }
    }
}
