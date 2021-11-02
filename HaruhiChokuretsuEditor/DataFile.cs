using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaruhiChokuretsuEditor
{
    public class DataFile : IFile
    {
        public int Index { get; set; }
        public int Offset { get; set; }
        public List<byte> Data { get; set; }
        public byte[] CompressedData { get; set; }

        public void Initialize(byte[] decompressedData, int offset)
        {
            Offset = offset;
            Data = decompressedData.ToList();
        }

        public byte[] GetBytes() => Data.ToArray();

        public override string ToString()
        {
            return $"{Index:X3} 0x{Offset:X8}";
        }
    }
}
