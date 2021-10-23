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
        public string Text { get; set; }

        public EventFile FromCompressedFile(string fileName)
        {
            return FromDecompressedData(Helpers.DecompressData(File.ReadAllBytes(fileName)));
        }

        public EventFile FromDecompressedData(byte[] data)
        {
            return new EventFile
            {
                Text = Encoding.GetEncoding(932).GetString(data) // SHIFT-JIS
            };
        }
    }

    public class DialogueLine
    {

    }
}
