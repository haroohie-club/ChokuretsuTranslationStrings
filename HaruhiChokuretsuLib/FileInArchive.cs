using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaruhiChokuretsuLib
{
    public partial class FileInArchive
    {
        public uint MagicInteger { get; set; }
        public int Index { get; set; }
        public int Offset { get; set; }
        public int Length { get; set; }
        public List<byte> Data { get; set; }
        public byte[] CompressedData { get; set; }

        public virtual void Initialize(byte[] compressedData, int offset)
        {
        }
        public virtual byte[] GetBytes()
        {
            return null;
        }

        public FileInArchive()
        {
        }
    }

    public static class FileManager<T>
        where T : FileInArchive, new()
    {
        public static T FromCompressedData(byte[] compressedData, int offset = 0)
        {
            T created = new();
            created.Initialize(Helpers.DecompressData(compressedData), offset);
            return created;
        }
    }
}
