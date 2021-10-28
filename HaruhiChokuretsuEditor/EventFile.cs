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
        public int Index { get; set; }
        public int Offset { get; set; }
        public List<byte> Data { get; set; } = new();
        public List<int> FrontPointers { get; set; } = new();
        public List<int> EndPointers { get; set; } = new();
        public List<int> EndPointerPointers { get; set; } = new();
        public string Title { get; set; }

        public List<DialogueLine> DialogueLines { get; set; } = new();

        public static EventFile FromCompressedFile(string fileName)
        {
            return FromCompressedData(File.ReadAllBytes(fileName));
        }

        public static EventFile FromCompressedData(byte[] compressedData, int offset = 0)
        {
            return new EventFile(Helpers.DecompressData(compressedData), offset);
        }

        public EventFile(byte[] decompressedData, int offset = 0)
        {
            Offset = offset;
            Data = decompressedData.ToList();

            int numFrontPointers = BitConverter.ToInt32(decompressedData.Take(4).ToArray());
            for (int i = 0; i < numFrontPointers; i++)
            {
                FrontPointers.Add(BitConverter.ToInt32(decompressedData.Skip(0x0C + (0x08 * i)).Take(4).ToArray()));
            }

            int pointerToNumEndPointers = BitConverter.ToInt32(decompressedData.Skip(4).Take(4).ToArray());
            int numEndPointers = BitConverter.ToInt32(decompressedData.Skip(pointerToNumEndPointers).Take(4).ToArray());
            for (int i = 0; i < numEndPointers; i++)
            {
                EndPointers.Add(BitConverter.ToInt32(decompressedData.Skip(pointerToNumEndPointers + (0x04 * (i + 1))).Take(4).ToArray()));
            }

            EndPointerPointers = EndPointers.Select(p => BitConverter.ToInt32(decompressedData.Skip(p).Take(4).ToArray())).ToList();

            DialogueLines = EndPointerPointers.Select(p => new DialogueLine(p, decompressedData)).ToList();

            Title = Encoding.ASCII.GetString(decompressedData.Skip(FrontPointers.Last()).TakeWhile(b => b != 0x00).ToArray());
        }

        public byte[] GetBytes() => Data.ToArray();

        public void ShiftPointers(int shiftLocation, int shiftAmount)
        {

        }

        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(Title))
            {
                return $"{Index:X3} 0x{Offset:X8} '{Title}'";
            }
            else if (DialogueLines.Count > 0)
            {
                return $"{Index:X3} 0x{Offset:X8}, Line 1: {DialogueLines[0].Text}";
            }
            else
            {
                return $"{Index:X3} 0x{Offset:X8}";
            }
        }
    }

    public class DialogueLine
    {
        public int Pointer { get; set; }
        public byte[] Data { get; set; }
        public string Text { get => Encoding.GetEncoding("Shift-JIS").GetString(Data); set => Data = Encoding.GetEncoding(932).GetBytes(value); }
        public int Length => Data.Length;

        public DialogueLine(int pointer, byte[] file)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Pointer = pointer;
            Data = file.Skip(pointer).TakeWhile(b => b != 0x00).ToArray();
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
