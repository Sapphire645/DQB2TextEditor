using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DQB2TextEditor.Linkdata
{
    internal class LDFile
    {
        private byte[] _data;
        private bool _isCompressed;
        private UInt32 _uncompressedSize;

        private WeakReference<byte[]> _uncompressedData;
        protected byte[] uncompressedData
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
        }
        public LDFile(byte[] data, bool isCompressed, UInt32 uncompressedSize)
        {
            _data = data;
            _isCompressed = isCompressed;
            _uncompressedSize = uncompressedSize;
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
                        zlib.CopyTo(output, (int)_uncompressedSize);
                        zlib.Flush();
                        result = output.ToArray();
                    }
                }
            }
            return result;
        }
    }
}
