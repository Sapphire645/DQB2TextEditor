using DQB2TextEditor.InfoReading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DQB2TextEditor.Windows
{
    public class Character
    {
        private ushort id;
        public ushort ID { get { return id; } }
        private string name;
        public string Name { get { return InformationReading.GetCharNames(id, ViewModel._currentLanguage); } }

        public string FullName => id + ": " + Name;
    }

    public class Boolext2
    {
        public bool Value { get; private set; }
    }
    public class Bool0
    {
        public bool Value { get; private set; }
    }
}
