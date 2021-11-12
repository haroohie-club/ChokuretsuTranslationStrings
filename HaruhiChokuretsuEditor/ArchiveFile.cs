using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaruhiChokuretsuEditor
{
    public class ArchiveFile<T>
        where T : IFile, new()
    {
        public const int FirstHeaderPointerOffset = 0x1C;

        public byte[] Header { get; set; }

        public int NumItems { get; set; }
        public int HeaderLength { get; set; }
        public int OffsetMsbMultiplier { get; set; }
        public int OffsetLsbMultiplier { get; set; }
        public int OffsetLsbAnd { get; set; }
        public int OffsetMsbShift { get; set; }
        public List<uint> HeaderPointers { get; set; } = new();
        public List<uint> SecondHeaderNumbers { get; set; } = new();
        public List<T> Files { get; set; } = new();

        public static ArchiveFile<T> FromFile(string fileName)
        {
            byte[] evtBytes = File.ReadAllBytes(fileName);
            return new ArchiveFile<T>(evtBytes);
        }

        public ArchiveFile(byte[] fileSystemBytes)
        {
            int endOfHeader = 0x00;
            for (int i = 0; i < fileSystemBytes.Length - 0x10; i++)
            {
                if (fileSystemBytes.Skip(i).Take(0x10).All(b => b == 0x00))
                {
                    endOfHeader = i;
                    break;
                }
            }
            int numZeroes = fileSystemBytes.Skip(endOfHeader).TakeWhile(b => b == 0x00).Count();
            int firstFileOffset = endOfHeader + numZeroes;

            Header = fileSystemBytes.Take(firstFileOffset).ToArray();

            NumItems = BitConverter.ToInt32(fileSystemBytes.Take(4).ToArray());

            OffsetMsbMultiplier = BitConverter.ToInt32(fileSystemBytes.Skip(0x04).Take(4).ToArray());
            OffsetLsbMultiplier = BitConverter.ToInt32(fileSystemBytes.Skip(0x08).Take(4).ToArray());

            OffsetLsbAnd = BitConverter.ToInt32(fileSystemBytes.Skip(0x10).Take(4).ToArray());
            OffsetMsbShift = BitConverter.ToInt32(fileSystemBytes.Skip(0x0C).Take(4).ToArray());

            HeaderLength = BitConverter.ToInt32(fileSystemBytes.Skip(0x1C).Take(4).ToArray()) + (NumItems * 2 + 8) * 4;
            for (int i = FirstHeaderPointerOffset; i < (NumItems * 4) + 0x20; i += 4)
            {
                HeaderPointers.Add(BitConverter.ToUInt32(fileSystemBytes.Skip(i).Take(4).ToArray()));
            }
            int firstNextPointer = FirstHeaderPointerOffset + HeaderPointers.Count * 4;
            for (int i = firstNextPointer; i < (NumItems * 4) + firstNextPointer; i += 4)
            {
                SecondHeaderNumbers.Add(BitConverter.ToUInt32(fileSystemBytes.Skip(i).Take(4).ToArray()));
            }

            for (int i = firstFileOffset; i < fileSystemBytes.Length;)
            {
                int offset = i;
                List<byte> fileBytes = new();
                byte[] nextLine = fileSystemBytes.Skip(i).Take(0x10).ToArray();
                for (i += 0x10; !nextLine.All(b => b == 0x00); i += 0x10)
                {
                    fileBytes.AddRange(nextLine);
                    nextLine = fileSystemBytes.Skip(i).Take(0x10).ToArray();
                }
                if (fileBytes.Count > 0)
                {
                    fileBytes.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });

                    T file = new();
                    try
                    {
                        file = FileManager<T>.FromCompressedData(fileBytes.ToArray(), offset);
                    }
                    catch (IndexOutOfRangeException)
                    {
                        Console.WriteLine($"Failed to parse file at 0x{i:X8} due to index out of range exception (most likely during decompression)");
                    }
                    file.Offset = offset;
                    file.MagicInteger = GetMagicInteger(file.Offset);
                    file.Index = GetFileIndex(file.MagicInteger);
                    if (file.Index == 0x9A)
                    {
                        Console.WriteLine("Here");
                    }
                    file.Length = GetFileLength(file.MagicInteger);
                    file.CompressedData = fileBytes.ToArray();
                    Files.Add(file);
                }
                byte[] zeroes = fileSystemBytes.Skip(i).TakeWhile(b => b == 0x00).ToArray();
                i += zeroes.Length;
            }
        }

        public uint GetMagicInteger(int offset)
        {
            uint msbToSearchFor = (uint)(offset / OffsetMsbMultiplier) << OffsetMsbShift;
            return HeaderPointers.FirstOrDefault(p => (p & 0xFFFF0000) == msbToSearchFor);
        }

        public int GetFileIndex(uint magicInteger)
        {
            return HeaderPointers.IndexOf(magicInteger);
        }

        public int GetFileLength(uint magicInteger)
        {
            // absolutely unhinged routine
            int magicLengthInt = 0x7FF + (int)((magicInteger & (uint)OffsetLsbAnd) * (uint)OffsetLsbMultiplier);
            int standardLengthIncrement = 0x800;
            if (magicLengthInt < standardLengthIncrement)
            {
                standardLengthIncrement = magicLengthInt;
                magicLengthInt = 0;
            }
            else
            {
                int magicLengthIntLeftShift = 0x1C;
                uint temp = (uint)magicLengthInt >> 0x04;
                if (standardLengthIncrement <= temp >> 0x0C)
                {
                    magicLengthIntLeftShift -= 0x10;
                    temp >>= 0x10;
                }
                if (standardLengthIncrement <= temp >> 0x04)
                {
                    magicLengthIntLeftShift -= 0x08;
                    temp >>= 0x08;
                }
                if (standardLengthIncrement <= temp)
                {
                    magicLengthIntLeftShift -= 0x04;
                    temp >>= 0x04;
                }

                magicLengthInt = (int)((uint)magicLengthInt << magicLengthIntLeftShift);
                standardLengthIncrement = 0 - standardLengthIncrement;

                bool carryFlag = Helpers.AddWillCauseCarry(magicLengthInt, magicLengthInt);
                magicLengthInt *= 2;

                int pcIncrement = (magicLengthIntLeftShift + magicLengthIntLeftShift * 2) * 4;

                for (; pcIncrement <= 0x174; pcIncrement += 0x0C)
                {
                    bool nextCarryFlag = Helpers.AddWillCauseCarry(standardLengthIncrement, (int)(temp << 1) + (carryFlag ? 1 : 0));
                    temp = (uint)standardLengthIncrement + (temp << 1) + (uint)(carryFlag ? 1 : 0);
                    carryFlag = nextCarryFlag;
                    if (!carryFlag)
                    {
                        temp -= (uint)standardLengthIncrement;
                    }
                    nextCarryFlag = Helpers.AddWillCauseCarry(magicLengthInt, magicLengthInt + (carryFlag ? 1 : 0));
                    magicLengthInt = (magicLengthInt * 2) + (carryFlag ? 1 : 0);
                    carryFlag = nextCarryFlag;
                }
            }

            return magicLengthInt * 0x800;
        }

        public int RecalculateFileOffset(T file, byte[] searchSet = null)
        {
            if (searchSet is null)
            {
                searchSet = Header;
            }
            return (int)(BitConverter.ToUInt32(searchSet.Skip(FirstHeaderPointerOffset + (file.Index * 4)).Take(4).ToArray()) >> OffsetMsbShift) * OffsetMsbMultiplier;
        }

        public byte[] GetBytes()
        {
            List<byte> bytes = new();

            bytes.AddRange(Header);
            for (int i = 0; i < Files.Count; i++)
            {
                byte[] compressedBytes;
                if (Files[i].Data is null || Files[i].Data.Count == 0)
                {
                    compressedBytes = Files[i].CompressedData;
                }
                else
                {
                    compressedBytes = Helpers.CompressData(Files[i].GetBytes());
                }
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
                        Header[pointerOffset + 2] = newPointer[2];
                        Header[pointerOffset + 3] = newPointer[3];
                        Files[i + 1].Offset = RecalculateFileOffset(Files[i + 1], bytes.ToArray());
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
