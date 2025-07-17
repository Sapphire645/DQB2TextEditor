using DQB2TextEditor.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DQB2TextEditor.Linkdata
{
    internal class LDFolder_TextData : LDFolder
    {

        protected override FolderType Type => FolderType.TextData;
        public LDFolder_TextData(LINKDATAEntry Entry) : base(Entry)
        {

        }


        public string[] GetTextLinesPreview()
        {
            //Get my files
            LDFile[] data = Files;
            //Ask the file for the line.
            return ((LDFile_TextData)data[0]).Lines;

        }
    }
}
