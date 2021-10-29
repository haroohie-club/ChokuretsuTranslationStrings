using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaruhiChokuretsuEditor
{
    public class EvtFile
    {
        public const int FirstHeaderPointerOffset = 0x1C;
        public const int FirstEventOffset = 0x2800;

        public byte[] Header { get; set; }

        public int NumItems { get; set; }
        public int HeaderLength { get; set; }
        public int OffsetMsbMultiplier { get; set; }
        public int OffsetLsbMultiplier { get; set; }
        public int OffsetLsbAnd { get; set; }
        public int OffsetMsbShift { get; set; }
        public List<uint> HeaderPointers { get; set; } = new();
        public List<EventFile> EventFiles { get; set; } = new();

        public static EvtFile FromFile(string fileName, out string log)
        {
            byte[] evtBytes = File.ReadAllBytes(fileName);
            return new EvtFile(evtBytes, out log);
        }

        public EvtFile(byte[] evtBytes, out string log)
        {
            log = "";

            Header = evtBytes.Take(FirstEventOffset).ToArray();

            NumItems = BitConverter.ToInt32(evtBytes.Take(4).ToArray());

            OffsetMsbMultiplier = BitConverter.ToInt32(evtBytes.Skip(0x04).Take(4).ToArray());
            OffsetLsbMultiplier = BitConverter.ToInt32(evtBytes.Skip(0x08).Take(4).ToArray());

            OffsetLsbAnd = BitConverter.ToInt32(evtBytes.Skip(0x10).Take(4).ToArray());
            OffsetMsbShift = BitConverter.ToInt32(evtBytes.Skip(0x0C).Take(4).ToArray());

            HeaderLength = BitConverter.ToInt32(evtBytes.Skip(0x1C).Take(4).ToArray()) + (NumItems * 2 + 8) * 4;
            for (int i = FirstHeaderPointerOffset; i < (NumItems * 4) + 0x20; i += 4)
            {
                HeaderPointers.Add(BitConverter.ToUInt32(evtBytes.Skip(i).Take(4).ToArray()));
            }

            for (int i = FirstEventOffset; i < evtBytes.Length;)
            {
                int offset = i;
                List<byte> eventBytes = new();
                byte[] nextLine = evtBytes.Skip(i).Take(0x10).ToArray();
                for (i += 0x10; !nextLine.All(b => b == 0x00); i += 0x10)
                {
                    eventBytes.AddRange(nextLine);
                    nextLine = evtBytes.Skip(i).Take(0x10).ToArray();
                }
                if (eventBytes.Count > 0)
                {
                    eventBytes.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });

                    EventFile eventFile = EventFile.FromCompressedData(eventBytes.ToArray(), offset);
                    eventFile.Index = GetMagicIndex(eventFile.Offset);
                    eventFile.CompressedData = eventBytes.ToArray();
                    EventFiles.Add(eventFile);
                }
                byte[] zeroes = evtBytes.Skip(i).TakeWhile(b => b == 0x00).ToArray();
                i += zeroes.Length;
            }
        }

        private int GetMagicIndex(int offset)
        {
            uint msbToSearchFor = (uint)(offset / OffsetMsbMultiplier) << OffsetMsbShift;
            uint headerPointer = HeaderPointers.FirstOrDefault(p => (p & 0xFFFF0000) == msbToSearchFor);
            return HeaderPointers.IndexOf(headerPointer);
        }

        public int RecalculateEventOffset(EventFile eventFile, byte[] searchSet = null)
        {
            if (searchSet is null)
            {
                searchSet = Header;
            }
            return (int)(BitConverter.ToUInt32(searchSet.Skip(FirstHeaderPointerOffset + (eventFile.Index * 4)).Take(4).ToArray()) >> OffsetMsbShift) * OffsetMsbMultiplier;
        }

        public byte[] GetBytes(bool compressedData = false)
        {
            List<byte> bytes = new();

            bytes.AddRange(Header);
            for (int i = 0; i < EventFiles.Count; i++)
            {
                byte[] compressedBytes;
                compressedBytes = Helpers.CompressData(EventFiles[i].GetBytes());
                bytes.AddRange(compressedBytes);
                if (i < EventFiles.Count - 1)
                {
                    int pointerShift = 0;
                    while (bytes.Count % 0x10 != 0)
                    {
                        bytes.Add(0);
                    }
                    if (bytes.Count > EventFiles[i + 1].Offset)
                    {
                        pointerShift = ((bytes.Count - EventFiles[i + 1].Offset) / OffsetMsbMultiplier) + 1;
                    }
                    if (pointerShift > 0)
                    {
                        byte[] newPointer = BitConverter.GetBytes((uint)((EventFiles[i + 1].Offset / OffsetMsbMultiplier) + pointerShift) << OffsetMsbShift);
                        int pointerOffset = FirstHeaderPointerOffset + (EventFiles[i + 1].Index * 4);
                        bytes[pointerOffset + 2] = newPointer[2];
                        bytes[pointerOffset + 3] = newPointer[3];
                        EventFiles[i + 1].Offset = RecalculateEventOffset(EventFiles[i + 1], bytes.ToArray());
                    }
                    while (bytes.Count < EventFiles[i + 1].Offset)
                    {
                        bytes.Add(0);
                    }
                }
            }
            while (bytes.Count % 0x800 != 0)
            {
                bytes.Add(0);
            }

            return bytes.ToArray();
        }
    }
}
