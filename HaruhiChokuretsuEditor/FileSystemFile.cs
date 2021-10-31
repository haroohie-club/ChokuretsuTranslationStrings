using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaruhiChokuretsuEditor
{
    public class FileSystemFile<T>
        where T : IFile, new()
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
        public List<T> Files { get; set; } = new();

        public static FileSystemFile<T> FromFile(string fileName)
        {
            byte[] evtBytes = File.ReadAllBytes(fileName);
            return new FileSystemFile<T>(evtBytes);
        }

        public FileSystemFile(byte[] fileBytes)
        {
            Header = fileBytes.Take(FirstEventOffset).ToArray();

            NumItems = BitConverter.ToInt32(fileBytes.Take(4).ToArray());

            OffsetMsbMultiplier = BitConverter.ToInt32(fileBytes.Skip(0x04).Take(4).ToArray());
            OffsetLsbMultiplier = BitConverter.ToInt32(fileBytes.Skip(0x08).Take(4).ToArray());

            OffsetLsbAnd = BitConverter.ToInt32(fileBytes.Skip(0x10).Take(4).ToArray());
            OffsetMsbShift = BitConverter.ToInt32(fileBytes.Skip(0x0C).Take(4).ToArray());

            HeaderLength = BitConverter.ToInt32(fileBytes.Skip(0x1C).Take(4).ToArray()) + (NumItems * 2 + 8) * 4;
            for (int i = FirstHeaderPointerOffset; i < (NumItems * 4) + 0x20; i += 4)
            {
                HeaderPointers.Add(BitConverter.ToUInt32(fileBytes.Skip(i).Take(4).ToArray()));
            }

            for (int i = FirstEventOffset; i < fileBytes.Length;)
            {
                int offset = i;
                List<byte> eventBytes = new();
                byte[] nextLine = fileBytes.Skip(i).Take(0x10).ToArray();
                for (i += 0x10; !nextLine.All(b => b == 0x00); i += 0x10)
                {
                    eventBytes.AddRange(nextLine);
                    nextLine = fileBytes.Skip(i).Take(0x10).ToArray();
                }
                if (eventBytes.Count > 0)
                {
                    eventBytes.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });

                    T file = FileManager<T>.FromCompressedData(eventBytes.ToArray(), offset);
                    file.Index = GetMagicIndex(file.Offset);
                    file.CompressedData = eventBytes.ToArray();
                    Files.Add(file);
                }
                byte[] zeroes = fileBytes.Skip(i).TakeWhile(b => b == 0x00).ToArray();
                i += zeroes.Length;
            }
        }

        public int GetMagicIndex(int offset)
        {
            uint msbToSearchFor = (uint)(offset / OffsetMsbMultiplier) << OffsetMsbShift;
            uint headerPointer = HeaderPointers.FirstOrDefault(p => (p & 0xFFFF0000) == msbToSearchFor);
            return HeaderPointers.IndexOf(headerPointer);
        }

        public int RecalculateEventOffset(T eventFile, byte[] searchSet = null)
        {
            if (searchSet is null)
            {
                searchSet = Header;
            }
            return (int)(BitConverter.ToUInt32(searchSet.Skip(FirstHeaderPointerOffset + (eventFile.Index * 4)).Take(4).ToArray()) >> OffsetMsbShift) * OffsetMsbMultiplier;
        }

        public byte[] GetBytes()
        {
            List<byte> bytes = new();

            bytes.AddRange(Header);
            for (int i = 0; i < Files.Count; i++)
            {
                byte[] compressedBytes;
                compressedBytes = Helpers.CompressData(Files[i].GetBytes());
                bytes.AddRange(compressedBytes);
                if (i < Files.Count - 1)
                {
                    int pointerShift = 0;
                    while (bytes.Count % 0x10 != 0)
                    {
                        bytes.Add(0);
                    }
                    if (bytes.Count > Files[i + 1].Offset)
                    {
                        pointerShift = ((bytes.Count - Files[i + 1].Offset) / OffsetMsbMultiplier) + 1;
                    }
                    if (pointerShift > 0)
                    {
                        byte[] newPointer = BitConverter.GetBytes((uint)((Files[i + 1].Offset / OffsetMsbMultiplier) + pointerShift) << OffsetMsbShift);
                        int pointerOffset = FirstHeaderPointerOffset + (Files[i + 1].Index * 4);
                        bytes[pointerOffset + 2] = newPointer[2];
                        bytes[pointerOffset + 3] = newPointer[3];
                        Files[i + 1].Offset = RecalculateEventOffset(Files[i + 1], bytes.ToArray());
                    }
                    while (bytes.Count < Files[i + 1].Offset)
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
