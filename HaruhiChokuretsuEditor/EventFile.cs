using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources.NetStandard;
using System.Text;
using System.Threading.Tasks;

namespace HaruhiChokuretsuEditor
{
    public class EventFile : IFile
    {
        public int Index { get; set; }
        public int Offset { get; set; }
        public byte[] CompressedData { get; set; }
        public List<byte> Data { get; set; } = new();
        public List<int> FrontPointers { get; set; } = new();
        public List<int> EndPointers { get; set; } = new();
        public List<int> EndPointerPointers { get; set; } = new();
        public string Title { get; set; }

        public Dictionary<int, string> DramatisPersonae { get; set; } = new();
        public int DialogueSectionPointer { get; set; }
        public List<DialogueLine> DialogueLines { get; set; } = new();

        public EventFile()
        {
        }

        public void Initialize(byte[] decompressedData, int offset = 0)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Offset = offset;
            Data = decompressedData.ToList();

            int numFrontPointers = BitConverter.ToInt32(decompressedData.Take(4).ToArray());
            bool reachedDramatisPersonae = false;
            for (int i = 0; i < numFrontPointers; i++)
            {
                FrontPointers.Add(BitConverter.ToInt32(decompressedData.Skip(0x0C + (0x08 * i)).Take(4).ToArray()));
                uint pointerValue = BitConverter.ToUInt32(decompressedData.Skip(FrontPointers[i]).Take(4).ToArray());
                if (pointerValue > 0x10000000 || pointerValue == 0x8596) // 8596 is 妹 which is a valid character name, sadly lol
                {
                    reachedDramatisPersonae = true;
                    DramatisPersonae.Add(FrontPointers[i],
                        Encoding.GetEncoding("Shift-JIS").GetString(decompressedData.Skip(FrontPointers[i]).TakeWhile(b => b != 0x00).ToArray()));
                }
                else if (reachedDramatisPersonae)
                {
                    reachedDramatisPersonae = false;
                    DialogueSectionPointer = FrontPointers[i];
                }
            }

            for (int i = 0; DialogueSectionPointer + i < decompressedData.Length - 0x0C; i += 0x0C)
            {
                int character = BitConverter.ToInt32(decompressedData.Skip(DialogueSectionPointer + i).Take(4).ToArray());
                int speakerPointer = BitConverter.ToInt32(decompressedData.Skip(DialogueSectionPointer + i + 4).Take(4).ToArray());
                int dialoguePointer = BitConverter.ToInt32(decompressedData.Skip(DialogueSectionPointer + i + 8).Take(4).ToArray());

                if (character == 0 && speakerPointer == 0 && dialoguePointer == 0)
                {
                    break;
                }

                DramatisPersonae.TryGetValue(speakerPointer, out string speakerName);
                DialogueLines.Add(new DialogueLine((Speaker)character, speakerName, speakerPointer, dialoguePointer, Data.ToArray()));
            }

            int pointerToNumEndPointers = BitConverter.ToInt32(decompressedData.Skip(4).Take(4).ToArray());
            int numEndPointers = BitConverter.ToInt32(decompressedData.Skip(pointerToNumEndPointers).Take(4).ToArray());
            for (int i = 0; i < numEndPointers; i++)
            {
                EndPointers.Add(BitConverter.ToInt32(decompressedData.Skip(pointerToNumEndPointers + (0x04 * (i + 1))).Take(4).ToArray()));
            }

            EndPointerPointers = EndPointers.Select(p => { int x = offset; return BitConverter.ToInt32(decompressedData.Skip(p).Take(4).ToArray()); }).ToList();

            int titlePointer = BitConverter.ToInt32(decompressedData.Skip(0x08).Take(4).ToArray());
            Title = Encoding.ASCII.GetString(decompressedData.Skip(titlePointer).TakeWhile(b => b != 0x00).ToArray());
        }

        public byte[] GetBytes() => Data.ToArray();

        public void ShiftPointers(int shiftLocation, int shiftAmount)
        {

        }

        public void WriteResxFile(string fileName)
        {
            using ResXResourceWriter resxWriter = new(fileName);
            for (int i = 0; i < DialogueLines.Count; i++)
            {
                resxWriter.AddResource(new ResXDataNode($"{i} {DialogueLines[i].Speaker} ({DialogueLines[i].SpeakerName})", DialogueLines[i].Text));
            }
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

        public int SpeakerPointer { get; set; }
        public Speaker Speaker { get; set; }
        public string SpeakerName { get; set; }

        public DialogueLine(Speaker speaker, string speakerName, int speakerPointer, int pointer, byte[] file)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Speaker = speaker;
            SpeakerName = speakerName;
            SpeakerPointer = speakerPointer;
            Pointer = pointer;
            Data = file.Skip(pointer).TakeWhile(b => b != 0x00).ToArray();
        }

        public override string ToString()
        {
            return Text;
        }
    }

    public enum Speaker
    {
        KYON = 0x01,
        HARUHI = 0x02,
        MIKURU = 0x03,
        NAGATO = 0x04,
        KOIZUMI = 0x05,
        KYON_SIS = 0x06,
        TSURUYA = 0x07,
        TANIGUCHI = 0x08,
        CLUB_PRES = 0x0A,
        CLUB_MEM_A = 0x0B,
        CLUB_MEM_B = 0x0C,
        CLUB_MEM_C = 0x0D,
        CLUB_MEM_D = 0x0E,
        OKABE = 0x0F,
        GROCER = 0x11,
        GIRL = 0x12,
        STRAY_CAT = 0x15,
        UNKNOWN = 0x16,
        INFO = 0x17,
        MONOLOGUE = 0x18,
        MAIL = 0x19,
    }
}
