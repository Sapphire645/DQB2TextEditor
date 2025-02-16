using System.IO;
using System.Windows.Shapes;


namespace DQB2TextEditor.code
{
    public class VersionInformation
    {
        public static List<CommandInfo> Commands = new List<CommandInfo>();

        public static string PlayerNameDefault => PlayerName == null || PlayerName.Equals("") ? (PlayerGender ? "Creatrix" : "Bildrick") : PlayerName;
        public static string PlayerName { get; set; }
        public static bool PlayerGender { get; set; } = false; //Are you Girl
        public static bool JapaneseMode { get; set; } 
        public List<String> ComboBoxVersions { get; set; } = new List<String>();
        public static String[] NamesPreview { get; private set; } = new String[1000];
        public static String[] ColourPreview { get; private set; } = new String[1000];
        public static String[] ItemsPreview { get; private set; } = new String[4000];
        public static bool Encrypted = false;
        public bool versionEncrypted = false;
        public String VersionFile { get; set; } = null;

        public List<String> Languages { get; private set; } = new List<String>();
        public ushort LanguageCount { get; private set; } = 0;

        public uint LINKDATASize { get; private set; } = 0;
        public int StartCutscene { get; private set; } = 0;
        public int StartText { get; private set; } = 0;
        public int DialogueCount { get; private set; } = 0;

        public List<(int, int)> IndividualText { get; private set; } = new List<(int, int)>();

        public int EndCutscene => StartCutscene + DialogueCount;
        public int TextCount => DialogueCount * LanguageCount;
        public int EndText => StartText + TextCount;

        public VersionInformation()
        {
            foreach (String version in Directory.GetFiles("info/versions/")) {
                var Curr = version.Split('/').Last();
                if (Curr.EndsWith(".txt")){
                    ComboBoxVersions.Add(Curr.Substring(0, Curr.Length-4));
                }
            };
            ReadCommands("info/CODE.txt");
            ReadPreviewData("info/Preview.txt");
        }

        public String getOpcode()
        {
            return System.IO.File.ReadAllText("info/OPCodes.txt");
        }
        public void Update()
        {
            ReadValues("info/versions/"+ VersionFile+".txt");
        }
        private void ReadPreviewData(string filename)
        {
            if (!System.IO.File.Exists(filename)) return;
            String[] lines = System.IO.File.ReadAllLines(filename);
            var Current = 0;
            foreach (String line in lines)
            {
                if (line[0] == '#') continue;
                var Values = line.Split("\t");
                if(Current < 1000)
                {
                    NamesPreview[Current] = Values[2];
                    ColourPreview[Current] = Values[1];
                }

                ItemsPreview[Current] = Values[0];
                Current++;
            }
        }
        private void ReadCommands(string filename)
        {
            if (!System.IO.File.Exists(filename)) return;
            String[] lines = System.IO.File.ReadAllLines(filename);
            var Current = -1;
            foreach (String line in lines)
            {
                var Line = line.Split("#")[0];
                Line = Line.Trim();
                var Values = Line.Split("\t");
                var NumberCommand = Values[0].Split(":");
                CommandInfo Command = new CommandInfo(uint.Parse(NumberCommand[0]), NumberCommand[1].Trim());
                if(Values.Length > 1)
                {
                    foreach (var Arg in Values[1].Split(","))
                    {
                        var NumberArg = Arg.Split(":");
                        Command.Arguments[uint.Parse(NumberArg[0])] = NumberArg[1].Trim();
                    }
                }
                Commands.Add(Command);
            }
        }

        public string GetVersionFromSize(uint size)
        {
            foreach(var Name in ComboBoxVersions)
            {
                foreach(var Line in System.IO.File.ReadAllLines("info/versions/" + Name + ".txt"))
                {
                    if (Line.StartsWith("-SIZE"))
                    {
                        uint.TryParse(Line.Split('\t').Last(), out var LS);
                        if (size == LS)
                        {
                            VersionFile = Name;
                            return Name;
                        }
                        break;
                    }
                }
            }
            return null;
        }
        public static CommandInfo FindCommand(uint command)
        {
            foreach(var Command in Commands)
                if (Command.Command == command) return Command;
            return null;
        }
        private void ReadValues(string filename)
        {
            if (!System.IO.File.Exists(filename)) return;
            String[] lines = System.IO.File.ReadAllLines(filename);
            var Current = -1;
            Encrypted = false;
            versionEncrypted = false;
            LanguageCount = 0;
            Languages.Clear();
            IndividualText.Clear();
            foreach (String line in lines)
            {
                if (line[0] == '#') continue;
                if (line.StartsWith("-SIZE"))
                {
                    uint.TryParse(line.Split('\t').Last(), out var LS);
                    LINKDATASize = LS;
                    continue;
                }
                if (line.StartsWith("-LAN")){
                    Current = 0;
                    continue;
                }
                if (line.StartsWith("-CUT")){
                    Current = 1;
                    continue;
                }
                if (line.StartsWith("-TXT"))
                {
                    Current = 2;
                    continue;
                }
                if (line.StartsWith("-ENCRYPTED"))
                {
                    Encrypted = true;
                    versionEncrypted = true;
                    continue;
                }
                switch (Current)
                {
                    case 0:
                        Languages.Add(line.Split('\t').Last());
                        LanguageCount++;
                        break;
                    case 1:
                        var Values = line.Split('\t');
                        if (Values[0].Equals("c"))
                        {
                            StartCutscene = int.Parse(Values[1].Split('-').First());
                            DialogueCount = ushort.Parse(Values[1].Split('-')[1]) - ushort.Parse(Values[1].Split('-')[0]);
                        }
                        else
                            if (Values[0].Equals("t"))
                            StartText = int.Parse(Values[1].Split('-').First());
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
        }
    }
    public class CommandInfo
    {
        public uint Command { get; private set; }
        public string Name;
        public string Added;
        public string NameFull => Added +" " + Name;
        public string[] Arguments = new string[11];

        public CommandInfo(uint command, string nameFull)
        {
            Command = command;
            String[] var = nameFull.Split(" ");
            if (var.Length == 2)
            {
                Added = var[0];
                Name = var[1].Trim();
            }
            else
            {
                Added = "";
                Name = var[0].Trim();
            }
        }
    }
}
