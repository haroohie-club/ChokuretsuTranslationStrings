using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaruhiChokuretsuEditor
{
    public class EventFile
    {
        public List<int> FrontPointers { get; set; } = new();
        public List<int> EndPointers { get; set; } = new();
        public List<int> EndPointerPointers { get; set; } = new();

        public static EventFile FromCompressedFile(string fileName)
        {
            return new EventFile(Helpers.DecompressData(File.ReadAllBytes(fileName)));
        }

        public EventFile(byte[] decompressedData)
        {
            int numFrontPointers = BitConverter.ToInt32(decompressedData.Take(4).ToArray());
            for (int i = 0; i < numFrontPointers; i++)
            {
                FrontPointers.Add(BitConverter.ToInt32(decompressedData.Skip(0x0C + 0x08 * i).Take(4).ToArray()));
            }

            int pointerToNumEndPointers = BitConverter.ToInt32(decompressedData.Skip(4).Take(4).ToArray());
            int numEndPointers = BitConverter.ToInt32(decompressedData.Skip(pointerToNumEndPointers).Take(4).ToArray());
            for (int i = 0; i < numEndPointers; i++)
            {
                EndPointers.Add(BitConverter.ToInt32(decompressedData.Skip(pointerToNumEndPointers + 0x04 * (i + 1)).Take(4).ToArray()));
            }

            EndPointerPointers = EndPointers.Select(p => BitConverter.ToInt32(decompressedData.Skip(p).Take(4).ToArray())).ToList();
        }
    }

    public class DialogueLine
    {

    }
}
