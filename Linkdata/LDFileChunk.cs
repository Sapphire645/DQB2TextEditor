using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.TextFormatting;

namespace DQB2TextEditor.Linkdata
{
    internal class LDFileChunk
    {
        private byte[] _data;
        public byte[] RawData => _data;
        private bool _isCompressed;

        public UInt32 CompressedSize => (UInt32)_data.Length;

        private WeakReference<byte[]> _uncompressedData;
        public byte[] uncompressedData
        {
            get
            {
                if (!_isCompressed) return _data;
                if (_uncompressedData == null || !_uncompressedData.TryGetTarget(out byte[] data))
                {
                    data = Decomp(_data);
                    _uncompressedData = new WeakReference<byte[]>(data);
                }
                return data;
            }
            //set
            //{
            //    if (!_isCompressed)
            //        _data = Comp(value);
            //    else
            //        _data = value;
            //    _uncompressedData = new WeakReference<byte[]>(value);
            //}
        }

        public LDFileChunk(bool isToCompress, byte[] uncompressedData)
        {
            if(isToCompress)
                _data = Comp(uncompressedData);
            else
                _data = uncompressedData;
            _isCompressed = isToCompress;
        }
        public LDFileChunk(byte[] data, bool isCompressed)
        {
            _data = data;
            _isCompressed = isCompressed;
        }

        private Byte[] Comp(Byte[] data)
        {
            Byte[] result = [];
            using (var input = new MemoryStream(data))
            {
                using (var output = new MemoryStream())
                {
                    using (var zlib = new System.IO.Compression.ZLibStream(output, System.IO.Compression.CompressionLevel.Fastest))
                    {
                        input.CopyTo(zlib);
                    }
                    result = output.ToArray();
                }
            }
            return result;
        }

        private byte[] Decomp(byte[] data)
        {
            byte[] result = [];
            using (var input = new MemoryStream(data))
            {
                using (var zlib = new System.IO.Compression.ZLibStream(input, System.IO.Compression.CompressionMode.Decompress))
                {
                    using (var output = new MemoryStream())
                    {
                        zlib.CopyTo(output);
                        zlib.Flush();
                        result = output.ToArray();
                    }
                }
            }
            return result;
        }
    }
}
