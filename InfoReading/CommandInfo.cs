using DQB2TextEditor.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DQB2TextEditor.InfoReading
{

    public class CommandInfo
    {
        public ushort Command { get; private set; }
        public string Name;
        public byte Type;
        public string NameFull => Name;
        public string[] Arguments = new string[11];
        public Type[] ArgumentTypes = new Type[11];

        public List<int> GetArgumentIndexes()
        {
            List<int> indexes = new List<int>();
            for(int i = 0; i < Arguments.Length; i++)
            {
                if(Arguments[i] != null)
                {
                    indexes.Add(i);
                }
            }
            return indexes;
        }
        public List<string> GetArgumentNames()
        {
            List<string> indexes = new List<string>();
            for (int i = 0; i < Arguments.Length; i++)
            {
                if (Arguments[i] != null)
                {
                    indexes.Add(Arguments[i]);
                }
            }
            return indexes;
        }

        public CommandInfo(string input)
        {
            var splits = input.Split('\t');
            var div = splits[0].Split(':');
            Command = ushort.Parse(div[0]);
            Name = div[1].Trim();
            Type = byte.Parse(splits[1].Trim());
            if(splits.Length > 2)
            {
                var args = splits[2].Split(',');
                foreach(var arg in args)
                {
                    var parts = arg.Split(':');
                    int index = int.Parse(parts[0].Trim());
                    Arguments[index] = parts[2];
                    switch(parts[1].Trim())
                    {
                        case "int":
                            ArgumentTypes[index] = typeof(int);
                            break;
                        case "float":
                            ArgumentTypes[index] = typeof(float);
                            break;
                        case "string":
                            ArgumentTypes[index] = typeof(string);
                            break;
                        case "byte":
                            ArgumentTypes[index] = typeof(byte);
                            break;
                        case "ushort":
                            ArgumentTypes[index] = typeof(ushort);
                            break;
                        case "short":
                            ArgumentTypes[index] = typeof(short);
                            break;
                        case "uint":
                            ArgumentTypes[index] = typeof(uint);
                            break;
                        case "long":
                            ArgumentTypes[index] = typeof(long);
                            break;
                        case "double":
                            ArgumentTypes[index] = typeof(double);
                            break;
                        case "chr":
                            ArgumentTypes[index] = typeof(Character);
                            break;
                        case "bool":
                            ArgumentTypes[index] = typeof(bool);
                            break;
                        case "boolext":
                            ArgumentTypes[index] = typeof(bool?);
                            break;
                        case "bool0":
                            ArgumentTypes[index] = typeof(Bool0);
                            break;
                        case "boolext2":
                            ArgumentTypes[index] = typeof(Boolext2);
                            break;
                        default:
                            ArgumentTypes[index] = typeof(int);
                            break;
                    }
                }
            }
                
        }
    }
}
