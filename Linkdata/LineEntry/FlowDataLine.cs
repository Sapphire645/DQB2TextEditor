using DQB2TextEditor.InfoReading;
using DQB2TextEditor.Windows;
using Microsoft.Windows.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DQB2TextEditor.Linkdata.LineEntry
{
    public class FlowDataLine
    {
        private byte[] data;

        public int argumentCount => types.Length;

        private string line;

        private Type[] types
        {
            get
            {
                if (InformationReading.Commands.TryGetValue(command, out CommandInfo cmdInfo))
                {
                    return cmdInfo.ArgumentTypes;
                }
                return new Type[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int) };
            }
        }
        public ushort command => BitConverter.ToUInt16(data, 0x2E);
        public bool unkonownFlag => BitConverter.ToInt32(data, 0x30) == 1;

        public (object, Type) GetArgument(int index)
        {
            return (Parse(index), types[index]);
        }
        public string Line => line;
        public string GetArgumentName(int index)
        {
            if(InformationReading.Commands.TryGetValue(command, out CommandInfo cmdInfo))
            {

               return cmdInfo.Arguments[index];


                
            }
            return "???";
        }
        public string GetCommandName()
        {
            if (InformationReading.Commands.TryGetValue(command, out CommandInfo cmdInfo))
            {
                return cmdInfo.NameFull;
            }
            return command +": ???";
        }
        public FlowDataLine(byte[] data, string line)
        {
            this.data = data;

            
            this.line = line;
        }

        private object Parse(int index)
        {
            switch (types[index])
            {
                case Type t when t == typeof(int):
                case Type a when a == typeof(Character):
                    return BitConverter.ToInt32(data, index * 4);
                case Type t when t == typeof(float):
                    return BitConverter.ToSingle(data, index * 4);
                case Type t when t == typeof(bool):
                case Type a when a == typeof(bool?):
                case Type b when b == typeof(Bool0):
                case Type c when c == typeof(Boolext2):
                    return (BitConverter.ToInt32(data, index * 4) == 1);
                default:
                    throw new InvalidOperationException($"Unsupported type: {types[index]}");
            }
        }

    }
}
