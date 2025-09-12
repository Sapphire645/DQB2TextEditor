using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DQB2TextEditor.InfoReading
{
    internal class LINKDATAVersion
    {
        public readonly String VersionName;
        public readonly String[] Languages;
        public byte LanguageCount => (byte)Languages.Length;

        public readonly ushort DialogueCount;
        //This might change so I dont want it to be public.
        private readonly uint _textPointer;
        private readonly uint _flowPointer;

        public uint StartFlowData => _flowPointer;
        public uint StartTextData => _textPointer;

        public readonly bool Encrypted = false;

        public readonly List<(int, int)> IndividualText = new List<(int, int)>();

        public LINKDATAVersion(string path)
        {
            VersionName = Path.GetFileNameWithoutExtension(path);
            if (!File.Exists(path)) throw new Exception($"What the fuck. Did you delete the {path} file why would you do thaaaaat");
            String[] lines = System.IO.File.ReadAllLines(path);
            var Current = -1;
            List<String> Languages = new List<String>();
            foreach (String line in lines)
            {
                if (line[0] == '#') continue;
                if (line[0] == '-')
                {
                    if (line.StartsWith("-LAN")) Current = 0;
                    if (line.StartsWith("-CUT")) Current = 1;
                    if (line.StartsWith("-TXT")) Current = 2;
                    if (line.StartsWith("-ENCRYPTED")) Encrypted = true;
                    continue;
                }
                switch (Current)
                {
                    case 0:
                        Languages.Add(line.Split('\t').Last());
                        break;
                    case 1:
                        var Values = line.Split('\t');
                        if (Values[0].Equals("c"))
                        {
                            _flowPointer = uint.Parse(Values[1].Split('-').First());
                            DialogueCount = (ushort)(ushort.Parse(Values[1].Split('-')[1]) - ushort.Parse(Values[1].Split('-')[0]));
                        }
                        else
                            if (Values[0].Equals("t"))
                            _textPointer = uint.Parse(Values[1].Split('-').First());
                        break;
                    case 2:
                        var ValuesT = line.Split('\t');
                        if (ValuesT[0].Equals("l"))
                        {
                            IndividualText.Add((int.Parse(ValuesT[1].Split('-').First()), ushort.Parse(ValuesT[1].Split('-')[1]) - ushort.Parse(ValuesT[1].Split('-')[0])));
                        }
                        break;
                    default:
                        break;
                }
            }
            this.Languages = Languages.ToArray();
        }
    }


    }
