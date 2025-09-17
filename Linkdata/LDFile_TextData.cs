using DQB2TextEditor.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;

namespace DQB2TextEditor.Linkdata
{
    internal class LDFile_TextData : LDFile
    {

        protected override FolderType Type => FolderType.TextData;

        public string[] Lines
        {
            get
            {
                if (_lines==null) ExtractData();
                return _lines;
            }
        }

        private string[] _lines = null;
        private uint sectionCount = 0;
        public LDFile_TextData(LINKDATAEntry Entry) : base(Entry)
        {

        }

        public string[] GetTextLinesPreview()
        {
            return Lines;
        }

        private void ExtractData()
        {
            try
            {
                byte[] uncompressData = uncompressedData;

                //Need to change how I apporach this.
                //This number counts the amount of "sections". A section is a colection of lines.
                //This matters in text, not dialogue.
                sectionCount = BitConverter.ToUInt32(uncompressData, 0x00);
                //Now I know how many offsets there are.
                uint offsetPointer = (uint)(sectionCount * 4 + 0x40);
                //The line pointer, however, depends on the real number of lines.
                //I can check by looking at where the pointer to the first line leads.
                uint linePointer = BitConverter.ToUInt32(uncompressData, (int)offsetPointer) + offsetPointer;

                uint lineCount = (uint)((linePointer - offsetPointer) / 4);
                //if (lineCount == 0) lineCount = sectionCount; //backup
                _lines = new string[lineCount];
                uint[] pointersLocal = new uint[lineCount];
                for (int i = 0; i < lineCount; i++)
                {
                    pointersLocal[i] = BitConverter.ToUInt32(uncompressData, (int)(offsetPointer + i * 4));
                }
                for (int i = 0; i < lineCount; i++)
                {
                    uint pointer = (uint)(pointersLocal[i] + offsetPointer + i * 4);
                    uint size = (uint)(uncompressData.Length - pointer);
                    if (size < 0) break;

                    if (i < lineCount - 1)
                    {
                        uint sizeCheck = pointersLocal[i + 1] - pointersLocal[i] + 4;
                        if (sizeCheck < size) size = sizeCheck;
                    }

                    _lines[i] = System.Text.Encoding.UTF8.GetString(uncompressData, (int)pointer, (int)size);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting text data: {ex.Message}");
                _lines = new string[2];
                _lines[0] = "<$cdef(68)>Error extracting text data.</color>";
                _lines[1] = ex.Message.ToString();
            }
        }
        public void UpdateLines(string[] lines)
        {
            SaveUncompressedFileData("TEST-OLD.bin");
            RewriteLines(lines);
            SaveUncompressedFileData("TEST-NEW.bin");
        }

        //I think instead of this I'll put this into a new File, then set the file to the folder
        //I'll work on that later.
        //I'd need to send over the section value.
        private void RewriteLines(string[] newLines)
        {
            //First I want to check if the amount of lines is the same. If it's not then send a warning.
            //This is relevant for the FlowData. I might want to check if encrypted....
            if (newLines.Length != Lines.Length)
            {
                MessageBox.Show("The amount of lines is different. If your version has ENCRYPTED FlowData you cannot change the indices of lines to match. Bad stuff, idk i'm sick rn can't think.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            //I want the section number still. Or do I make it editable? I guess I will eventually.

            uint offsetPointer = (uint)(sectionCount * 4 + 0x40);
            //how did this work now.... think brain, think!!!
            using (MemoryStream ms = new MemoryStream())
            {
                //There's the section number.
                ms.Write(BitConverter.GetBytes(sectionCount), 0, sizeof(uint));
                //Then from 0x04 to 0x3F is just 00000.
                for (int i = 0x04; i < 0x40; i++)
                {
                    ms.WriteByte(0x00);
                }
                //Now we write all of the pointer-to-pointer.
                for (int i = 0; i < sectionCount; i++)
                {
                    ms.Write(BitConverter.GetBytes(i), 0, sizeof(int));
                }
                //Now, the pointer size = 4*line number.
                uint AddedPointerOffset = (uint)(newLines.Length * 4);
                //For each line, get the size of the line,
                //add the offset, write, remove 4, repeat.
                //First line is directly after:
                ms.Write(BitConverter.GetBytes(AddedPointerOffset), 0, sizeof(uint));
                AddedPointerOffset -= 4;
                for (uint iter = 0; iter < newLines.Length - 1; iter++)
                {
                    if (!newLines[iter].EndsWith('\0')) newLines[iter] += '\0';
                    AddedPointerOffset += (uint)Encoding.UTF8.GetByteCount(newLines[iter]);
                    ms.Write(BitConverter.GetBytes(AddedPointerOffset), 0, sizeof(int));
                    AddedPointerOffset -= 4;
                }
                //Lines.
                foreach (string line in newLines)
                {
                    var text = Encoding.UTF8.GetBytes(line);
                    ms.Write(text, 0, text.Length);
                }
                UpdateFile(ms.ToArray());
            }
        }

    }
}
