using System;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DQB2TextEditor.code
{
    /* NEXT CLASS COMES FROM TURTLE-INSECT:
    https://github.com/turtle-insect/DQB2
    */
    public class IDX
	{
		public UInt64 Offset { get; set; }
		public UInt64 UncompressedSize { get; set; }
		public UInt64 CompressedSize { get; set; }
		public UInt64 IsCompressed { get; set; }
		public int Index { get; private set; }
		public IDX(int index) => Index = index;
	}

    public class Dialogue
	{
		public uint DialogueIndex { get; private set; }
        public int CutsceneIndex => CutsceneFile!= null ? CutsceneFile.Index : -1;
        public int TextIndex => TextFiles != null && TextFiles.Count > 0 ? TextFiles[ViewModel.CurrentSelectedLanguage].Index : -1;

        public IDX CutsceneFile { get; private set; }
		public List<IDX> TextFiles;

        public IDX CurrentTextFile => TextFiles != null && TextFiles.Count > 0 ? TextFiles[ViewModel.CurrentSelectedLanguage] : null;
        public string Preview { get; private set; }
		public Dialogue(uint index, IDX cutsceneFile)
		{
			DialogueIndex = index;
			CutsceneFile = cutsceneFile;
            Preview = "---";
        }
        public Dialogue(uint index, List<IDX> textFiles)
        {
            DialogueIndex = index;
            TextFiles = textFiles;
            Preview = "---";
        }
    }
    /* NEXT CLASSES COMES FROM TURTLE-INSECT:
    https://github.com/turtle-insect/DQB2
    */
    public class IDXzrc
    {
        public ObservableCollection<IDXzrcChunk> Chunks { get; private set; } = new ObservableCollection<IDXzrcChunk>();
        public UInt32 SplitSize { get; private set; }
        public UInt32 UncompressedSize { get; private set; }
        public void Read(String filename)
        {
            Chunks.Clear();
            if (!System.IO.File.Exists(filename)) return;

            Byte[] buffer = System.IO.File.ReadAllBytes(filename);
            if (BitConverter.ToUInt32(buffer, 0) == 0) return;

            SplitSize = BitConverter.ToUInt32(buffer, 0);
            UInt32 blockCount = BitConverter.ToUInt32(buffer, 4);
            UncompressedSize = BitConverter.ToUInt32(buffer, 8);
            UInt32 offset = 0x0C + blockCount * 4;
            for (int count = 0; count < blockCount; count++)
            {
                if (offset % 0x80 != 0) offset = (offset / 0x80 + 1) * 0x80;
                UInt32 size = BitConverter.ToUInt32(buffer, 0x0C + count * 4);
                var block = new IDXzrcChunk(size, offset);
                Chunks.Add(block);
                offset += size;
            }
        }

        public void ReadInternal(Byte[] buffer)
        {
            SplitSize = BitConverter.ToUInt32(buffer, 0);
            UInt32 blockCount = BitConverter.ToUInt32(buffer, 4);
            UncompressedSize = BitConverter.ToUInt32(buffer, 8);
            UInt32 offset = 0x0C + blockCount * 4;
            for (int count = 0; count < blockCount; count++)
            {
                if (offset % 0x80 != 0) offset = (offset / 0x80 + 1) * 0x80;
                UInt32 size = BitConverter.ToUInt32(buffer, 0x0C + count * 4);
                var block = new IDXzrcChunk(size, offset);
                Chunks.Add(block);
                offset += size;
            }
        }
    }
    public class IDXzrcChunk
    {
        public UInt32 Size { get; private set; }
        public UInt32 Offset { get; private set; }

        public IDXzrcChunk(UInt32 size, UInt32 offset)
        {
            Size = size;
            Offset = offset;
        }
    }
}
