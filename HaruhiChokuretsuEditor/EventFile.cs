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
        }
    }

    public class DialogueLine
    {

    }
}
