using DQB2TextEditor.InfoReading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DQB2TextEditor.Windows
{
    public abstract class ArgumentClass{
        protected int argument;
        public int Argument => argument;
        public string Display => getDisplay();

        public ArgumentClass(int arg)
        {
            argument = arg;
        }

        protected abstract string getDisplay();
    }
    public class Character : ArgumentClass
    {
        public string Name { get {

                if (argument == 0) return null;
                return InformationReading.GetCharNames((ushort)argument, ViewModel._currentLanguage);
            
            } }

        public string FullName => argument + ": " + Name;
        public Character(int arg) : base(arg)
        {
        }
        protected override string getDisplay()
        {
            return Name;
        }
        public override string ToString()
        {
            return "char";
        }
    }

    public class Bool01 : ArgumentClass
    {
        public Bool01(int arg) : base(arg)
        {
        }
        protected override string getDisplay()
        {
            switch (argument)
            {
                case 1:
                    return "true";
                case 0:
                    return "false";
                default:
                    return argument.ToString();
            }
        }

        public override string ToString()
        {
            return "Bool01";
        }
    }
    public class BoolN01 : ArgumentClass
    {
        public BoolN01(int arg) : base(arg)
        {
        }
        protected override string getDisplay()
        {
            switch (argument)
            {
                case 1:
                    return "true";
                case 0:
                    return "false";
                case -1:
                    return "null";
                default:
                    return argument.ToString();
            }
        }
        public override string ToString()
        {
            return "BoolN01";
        }
    }
    public class BoolN012 : ArgumentClass
    {
        public BoolN012(int arg) : base(arg)
        {
        }
        protected override string getDisplay()
        {
            switch (argument)
            {
                case 1:
                    return "true(1)";
                case 0:
                    return "false";
                case -1:
                    return "null";
                case 2:
                    return "true(2)";
                default:
                    return argument.ToString();
            }
        }
        public override string ToString()
        {
            return "BoolN012";
        }
    }

    //In the future, code for Chunk and such. Easier to see.
    public class Coordenate : ArgumentClass
    {
        public Coordenate(int arg) : base(arg)
        {
        }
        protected override string getDisplay()
        {
            return argument.ToString();
        }

        public override string ToString()
        {
            return "Coord";
        }
    }
}
