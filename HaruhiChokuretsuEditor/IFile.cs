using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Resources.NetStandard;

namespace HaruhiChokuretsuEditor
{
    public interface IFile
    {
        public uint MagicInteger { get; set; }
        public int Index { get; set; }
        public int Offset { get; set; }
        public int Length { get; set; }
        public List<byte> Data { get; set; }
        public byte[] CompressedData { get; set; }

        public abstract void Initialize(byte[] compressedData, int offset);
        public abstract byte[] GetBytes();
    }

    public static class FileManager<T>
        where T : IFile, new()
    {
        public static T FromCompressedData(byte[] compressedData, int offset = 0)
        {
            T created = new();
            created.Initialize(Helpers.DecompressData(compressedData), offset);
            return created;
        }
    }
}
