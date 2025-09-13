using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DQB2TextEditor.Linkdata
{
    internal class LDFile_TextData : LDFile
    {
        private bool initialized = false;
        public string[] Lines
        {
            get
            {
                if (!initialized) ExtractData();
                return _lines;
            }
        }

        private string[] _lines = Array.Empty<string>();
        public LDFile_TextData(byte[] data,UInt32 overflowmem) : base(data, true, overflowmem)
        {

        }

        private void ExtractData()
        {
            try
            {
                byte[] uncompressData = uncompressedData;

                //Need to change how I apporach this.
                //This number counts the amount of "sections". A section is a colection of lines.
                //This matters in text, not dialogue.
                uint sectionCount = BitConverter.ToUInt32(uncompressData, 0x00); 
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
            finally
            {
                initialized = true;
            }

        }
    }
}
