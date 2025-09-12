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
        public LDFile_TextData(byte[] data) : base(data, true)
        {

        }

        private void ExtractData()
        {
            try
            {
                byte[] uncompressData = uncompressedData;

                uint offsetPointer = (uint)(BitConverter.ToUInt16(uncompressData, 0x00) * 4 + 0x40);
                uint linePointer = BitConverter.ToUInt16(uncompressData, (int)offsetPointer) + offsetPointer;

                uint lineCount = (uint)((linePointer - offsetPointer) / 4);
                _lines = new string[lineCount];
                uint[] pointersLocal = new uint[lineCount];
                for (int i = 0; i < lineCount; i++)
                {
                    pointersLocal[i] = BitConverter.ToUInt16(uncompressData, (int)(offsetPointer + i * 4));
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
