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
            byte[] uncompressData = uncompressedData;

            string a = "";
            foreach (byte b in uncompressData) a += b.ToString("X2") + " ";
            Console.WriteLine($"Reading {a}");

            int offsetPointer = BitConverter.ToInt32(uncompressData, 0x00) * 4 + 0x40;
            int linePointer = BitConverter.ToInt32(uncompressData, (int)offsetPointer) + offsetPointer;

            ushort lineCount = (ushort)((linePointer - offsetPointer) / 4);
            _lines = new string[lineCount];
            int[] pointersLocal = new int[lineCount];
            for (int i = 0; i < lineCount; i++)
            {
                pointersLocal[i] = BitConverter.ToInt32(uncompressData, offsetPointer + i * 4);
            }
            for (int i = 0; i < lineCount - 1; i++)
            {
                int size = pointersLocal[i + 1] - pointersLocal[i] + 4;
                int pointer = pointersLocal[i] + offsetPointer + i * 4;
                _lines[i] = System.Text.Encoding.UTF8.GetString(uncompressData, pointer, size);
            }
            int pointerFinal = pointersLocal[lineCount-1] + offsetPointer + (lineCount - 1) * 4;
            int sizeFinal = uncompressData.Length - pointerFinal;
            _lines[lineCount - 1] = System.Text.Encoding.UTF8.GetString(uncompressData, pointerFinal, sizeFinal);
        }





    }
}
