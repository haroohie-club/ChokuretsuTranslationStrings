using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources.NetStandard;
using System.Text;
using System.Threading.Tasks;

namespace HaruhiChokuretsuLib.Archive
{
    public class EventFile : FileInArchive
    {
        public List<int> FrontPointers { get; set; } = new();
        public int PointerToNumEndPointers { get; set; }
        public List<int> EndPointers { get; set; } = new();
        public List<int> EndPointerPointers { get; set; } = new();
        public string Title { get; set; }

        public Dictionary<int, string> DramatisPersonae { get; set; } = new();
        public int DialogueSectionPointer { get; set; }
        public List<DialogueLine> DialogueLines { get; set; } = new();

        public EventFile()
        {
        }

        public struct CharacterWithSpacing
        {
            public char Character { get; set; }
            public int Width { get; set; }
        }

        private const int DIALOGUE_LINE_LENGTH = 230;
        private static readonly Dictionary<char, CharacterWithSpacing> CharacterToLengthMap = new()
        {
            { '0', new CharacterWithSpacing { Character = '０', Width = 6 } },
            { '1', new CharacterWithSpacing { Character = '１', Width = 4 } },
            { '2', new CharacterWithSpacing { Character = '２', Width = 6 } },
            { '3', new CharacterWithSpacing { Character = '３', Width = 6 } },
            { '4', new CharacterWithSpacing { Character = '４', Width = 7 } },
            { '5', new CharacterWithSpacing { Character = '５', Width = 6 } },
            { '6', new CharacterWithSpacing { Character = '６', Width = 6 } },
            { '7', new CharacterWithSpacing { Character = '７', Width = 6 } },
            { '8', new CharacterWithSpacing { Character = '８', Width = 6 } },
            { '9', new CharacterWithSpacing { Character = '９', Width = 6 } },
            { 'A', new CharacterWithSpacing { Character = 'Ａ', Width = 6 } },
            { 'B', new CharacterWithSpacing { Character = 'Ｂ', Width = 6 } },
            { 'C', new CharacterWithSpacing { Character = 'Ｃ', Width = 7 } },
            { 'D', new CharacterWithSpacing { Character = 'Ｄ', Width = 6 } },
            { 'E', new CharacterWithSpacing { Character = 'Ｅ', Width = 6 } },
            { 'F', new CharacterWithSpacing { Character = 'Ｆ', Width = 6 } },
            { 'G', new CharacterWithSpacing { Character = 'Ｇ', Width = 7 } },
            { 'H', new CharacterWithSpacing { Character = 'Ｈ', Width = 6 } },
            { 'I', new CharacterWithSpacing { Character = 'Ｉ', Width = 5 } },
            { 'J', new CharacterWithSpacing { Character = 'Ｊ', Width = 6 } },
            { 'K', new CharacterWithSpacing { Character = 'Ｋ', Width = 7 } },
            { 'L', new CharacterWithSpacing { Character = 'Ｌ', Width = 7 } },
            { 'M', new CharacterWithSpacing { Character = 'Ｍ', Width = 6 } },
            { 'N', new CharacterWithSpacing { Character = 'Ｎ', Width = 6 } },
            { 'O', new CharacterWithSpacing { Character = 'Ｏ', Width = 7 } },
            { 'P', new CharacterWithSpacing { Character = 'Ｐ', Width = 6 } },
            { 'Q', new CharacterWithSpacing { Character = 'Ｑ', Width = 7 } },
            { 'R', new CharacterWithSpacing { Character = 'Ｒ', Width = 6 } },
            { 'S', new CharacterWithSpacing { Character = 'Ｓ', Width = 6 } },
            { 'T', new CharacterWithSpacing { Character = 'Ｔ', Width = 6 } },
            { 'U', new CharacterWithSpacing { Character = 'Ｕ', Width = 6 } },
            { 'V', new CharacterWithSpacing { Character = 'Ｖ', Width = 6 } },
            { 'W', new CharacterWithSpacing { Character = 'Ｗ', Width = 6 } },
            { 'X', new CharacterWithSpacing { Character = 'Ｘ', Width = 6 } },
            { 'Y', new CharacterWithSpacing { Character = 'Ｙ', Width = 6 } },
            { 'Z', new CharacterWithSpacing { Character = 'Ｚ', Width = 6 } },
            { 'a', new CharacterWithSpacing { Character = 'ａ', Width = 7 } },
            { 'b', new CharacterWithSpacing { Character = 'ｂ', Width = 6 } },
            { 'c', new CharacterWithSpacing { Character = 'ｃ', Width = 6 } },
            { 'd', new CharacterWithSpacing { Character = 'ｄ', Width = 6 } },
            { 'e', new CharacterWithSpacing { Character = 'ｅ', Width = 6 } },
            { 'f', new CharacterWithSpacing { Character = 'ｆ', Width = 5 } },
            { 'g', new CharacterWithSpacing { Character = 'ｇ', Width = 7 } },
            { 'h', new CharacterWithSpacing { Character = 'ｈ', Width = 6 } },
            { 'i', new CharacterWithSpacing { Character = 'ｉ', Width = 4 } },
            { 'j', new CharacterWithSpacing { Character = 'ｊ', Width = 4 } },
            { 'k', new CharacterWithSpacing { Character = 'ｋ', Width = 6 } },
            { 'l', new CharacterWithSpacing { Character = 'ｌ', Width = 3 } },
            { 'm', new CharacterWithSpacing { Character = 'ｍ', Width = 6 } },
            { 'n', new CharacterWithSpacing { Character = 'ｎ', Width = 6 } },
            { 'o', new CharacterWithSpacing { Character = 'ｏ', Width = 6 } },
            { 'p', new CharacterWithSpacing { Character = 'ｐ', Width = 6 } },
            { 'q', new CharacterWithSpacing { Character = 'ｑ', Width = 6 } },
            { 'r', new CharacterWithSpacing { Character = 'ｒ', Width = 5 } },
            { 's', new CharacterWithSpacing { Character = 'ｓ', Width = 6 } },
            { 't', new CharacterWithSpacing { Character = 'ｔ', Width = 6 } },
            { 'u', new CharacterWithSpacing { Character = 'ｕ', Width = 6 } },
            { 'v', new CharacterWithSpacing { Character = 'ｖ', Width = 6 } },
            { 'w', new CharacterWithSpacing { Character = 'ｗ', Width = 6 } },
            { 'x', new CharacterWithSpacing { Character = 'ｘ', Width = 6 } },
            { 'y', new CharacterWithSpacing { Character = 'ｙ', Width = 6 } },
            { 'z', new CharacterWithSpacing { Character = 'ｚ', Width = 6 } },
            { '@', new CharacterWithSpacing { Character = '＠', Width = 7 } },
            { ',', new CharacterWithSpacing { Character = '、', Width = 4 } },
            { '。', new CharacterWithSpacing { Character = '。', Width = 5 } },
            { '.', new CharacterWithSpacing { Character = '．', Width = 4 } },
            { '・', new CharacterWithSpacing { Character = '・', Width = 9 } },
            { ':', new CharacterWithSpacing { Character = '：', Width = 5 } },
            { '?', new CharacterWithSpacing { Character = '？', Width = 6 } },
            { '!', new CharacterWithSpacing { Character = '！', Width = 4 } },
            { '_', new CharacterWithSpacing { Character = '＿', Width = 8 } },
            { '-', new CharacterWithSpacing { Character = '―', Width = 6 } },
            { '~', new CharacterWithSpacing { Character = '～', Width = 7 } },
            { '…', new CharacterWithSpacing { Character = '…', Width = 3 } },
            { '\'', new CharacterWithSpacing { Character = '’', Width = 4 } },
            { '"', new CharacterWithSpacing { Character = '“', Width = 6 } },
            { '(', new CharacterWithSpacing { Character = '（', Width = 6 } },
            { ')', new CharacterWithSpacing { Character = '）', Width = 6 } },
            { '+', new CharacterWithSpacing { Character = '＋', Width = 6 } },
            { '×', new CharacterWithSpacing { Character = '×', Width = 12 } },
            { '=', new CharacterWithSpacing { Character = '＝', Width = 6 } },
            { '°', new CharacterWithSpacing { Character = '°', Width = 5 } },
            { '%', new CharacterWithSpacing { Character = '％', Width = 7 } },
            { '&', new CharacterWithSpacing { Character = '＆', Width = 7 } },
            { '☆', new CharacterWithSpacing { Character = '☆', Width = 14 } },
            { '■', new CharacterWithSpacing { Character = '■', Width = 14 } },
            { '♪', new CharacterWithSpacing { Character = '♪', Width = 11 } },
            { ' ', new CharacterWithSpacing { Character = '　', Width = 3 } },
        };

        public override void Initialize(byte[] decompressedData, int offset = 0)
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

            PointerToNumEndPointers = BitConverter.ToInt32(decompressedData.Skip(4).Take(4).ToArray());
            int numEndPointers = BitConverter.ToInt32(decompressedData.Skip(PointerToNumEndPointers).Take(4).ToArray());
            for (int i = 0; i < numEndPointers; i++)
            {
                EndPointers.Add(BitConverter.ToInt32(decompressedData.Skip(PointerToNumEndPointers + (0x04 * (i + 1))).Take(4).ToArray()));
            }

            EndPointerPointers = EndPointers.Select(p => { int x = offset; return BitConverter.ToInt32(decompressedData.Skip(p).Take(4).ToArray()); }).ToList();

            int titlePointer = BitConverter.ToInt32(decompressedData.Skip(0x08).Take(4).ToArray());
            Title = Encoding.ASCII.GetString(decompressedData.Skip(titlePointer).TakeWhile(b => b != 0x00).ToArray());
        }

        public override byte[] GetBytes() => Data.ToArray();

        public void EditDialogueLine(int index, string newText)
        {
            Edited = true;
            int oldLength = DialogueLines[index].Length + DialogueLines[index].NumPaddingZeroes;
            DialogueLines[index].Text = newText;
            DialogueLines[index].NumPaddingZeroes = 4 - (DialogueLines[index].Length % 4);
            int lengthDifference = DialogueLines[index].Length + DialogueLines[index].NumPaddingZeroes - oldLength;
            
            List<byte> toWrite = new();
            toWrite.AddRange(DialogueLines[index].Data);
            for (int i = 0; i < DialogueLines[index].NumPaddingZeroes; i++)
            {
                toWrite.Add(0);
            }
            
            Data.RemoveRange(DialogueLines[index].Pointer, oldLength);
            Data.InsertRange(DialogueLines[index].Pointer, toWrite);

            ShiftPointers(DialogueLines[index].Pointer, lengthDifference);
        }

        public void ShiftPointers(int shiftLocation, int shiftAmount)
        {
            for (int i = 0; i < FrontPointers.Count; i++)
            {
                if (FrontPointers[i] > shiftLocation)
                {
                    FrontPointers[i] += shiftAmount;
                    Data.RemoveRange(0x0C + (0x08 * i), 4);
                    Data.InsertRange(0x0C + (0x08 * i), BitConverter.GetBytes(FrontPointers[i]));
                }
            }
            if (PointerToNumEndPointers > shiftLocation)
            {
                PointerToNumEndPointers += shiftAmount;
                Data.RemoveRange(0x04, 4);
                Data.InsertRange(0x04, BitConverter.GetBytes(PointerToNumEndPointers));
            }
            for (int i = 0; i < EndPointers.Count; i++)
            {
                if (EndPointers[i] > shiftLocation)
                {
                    EndPointers[i] += shiftAmount;
                    Data.RemoveRange(PointerToNumEndPointers + 0x04 * (i + 1), 4);
                    Data.InsertRange(PointerToNumEndPointers + 0x04 * (i + 1), BitConverter.GetBytes(EndPointers[i]));
                }
            }
            for (int i = 0; i < EndPointerPointers.Count; i++)
            {
                if (EndPointerPointers[i] > shiftLocation)
                {
                    EndPointerPointers[i] += shiftAmount;
                    Data.RemoveRange(EndPointers[i], 4);
                    Data.InsertRange(EndPointers[i], BitConverter.GetBytes(EndPointerPointers[i]));
                }
            }
            foreach (DialogueLine dialogueLine in DialogueLines)
            {
                if (dialogueLine.Pointer > shiftLocation)
                {
                    dialogueLine.Pointer += shiftAmount;
                }
            }
        }

        public void WriteResxFile(string fileName)
        {
            using ResXResourceWriter resxWriter = new(fileName);
            for (int i = 0; i < DialogueLines.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(DialogueLines[i].Text) && DialogueLines[i].Length > 1)
                {
                    resxWriter.AddResource(new ResXDataNode($"{i:D4} ({Path.GetFileNameWithoutExtension(fileName)}) {DialogueLines[i].Speaker} ({DialogueLines[i].SpeakerName})",
                        DialogueLines[i].Text));
                }
            }
        }

        public void ImportResxFile(string fileName)
        {
            Edited = true;
            string resxContents = File.ReadAllText(fileName);
            resxContents = resxContents.Replace("System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                "System.Resources.NetStandard.ResXResourceWriter, System.Resources.NetStandard, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            resxContents = resxContents.Replace("System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                "System.Resources.NetStandard.ResXResourceReader, System.Resources.NetStandard, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            TextReader textReader = new StringReader(resxContents);

            using ResXResourceReader resxReader = new(textReader);
            foreach (DictionaryEntry d in resxReader)
            {
                int dialogueIndex = int.Parse(((string)d.Key)[0..4]);
                string dialogueText = (string)d.Value;

                dialogueText = dialogueText.Replace("\r\n", " ");
                dialogueText = dialogueText.Replace("\n", " ");

                int lineLength = 0;
                bool operatorActive = false;
                for (int i = 0; i < dialogueText.Length; i++)
                {
                    if (operatorActive)
                    {
                        if (dialogueText[i] >= '0' || dialogueText[i] <= '9')
                        {
                            continue;
                        }
                    }

                    if (dialogueText[i] == '$')
                    {
                        operatorActive = true;
                        continue;
                    }
                    else if (dialogueText[i] == '#')
                    {
                        i++; // skip W
                        operatorActive = true;
                        continue;
                    }

                    if (CharacterToLengthMap.ContainsKey(dialogueText[i]))
                    {
                        char newCharacter = CharacterToLengthMap[dialogueText[i]].Character;
                        if (dialogueText[i] == '"' && (i == dialogueText.Length - 1 || dialogueText[i + 1] == ' ' || dialogueText[i + 1] == '!' || dialogueText[i + 1] == '?'))
                        {
                            newCharacter = '”';
                        }
                        lineLength += CharacterToLengthMap[dialogueText[i]].Width;
                        dialogueText = dialogueText.Remove(i, 1);
                        dialogueText = dialogueText.Insert(i, $"{newCharacter}");
                    }

                    if (lineLength > DIALOGUE_LINE_LENGTH)
                    {
                        int indexOfMostRecentSpace = dialogueText[0..i].LastIndexOf('　'); // full-width space bc it's been replaced already
                        dialogueText = dialogueText.Remove(indexOfMostRecentSpace, 1);
                        dialogueText = dialogueText.Insert(indexOfMostRecentSpace, "\n");
                        lineLength = 0;
                    }
                }

                if (dialogueText.Count(c => c == '\n') > 1)
                {
                    Console.WriteLine($"File {Index} has dialogue line too long ({dialogueIndex}) (starting with: {dialogueText[0..Math.Min(5, dialogueText.Length - 1)]})");
                }

                EditDialogueLine(dialogueIndex, dialogueText);
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
        public int NumPaddingZeroes { get; set; }
        public string Text { get => Encoding.GetEncoding("Shift-JIS").GetString(Data); set => Data = Encoding.GetEncoding("Shift-JIS").GetBytes(value); }
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
            NumPaddingZeroes = file.Skip(pointer + Data.Length).TakeWhile(b => b == 0x00).Count();
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
        KUNIKIDA = 0x09,
        CLUB_PRES = 0x0A,
        CLUB_MEM_A = 0x0B,
        CLUB_MEM_B = 0x0C,
        CLUB_MEM_C = 0x0D,
        CLUB_MEM_D = 0x0E,
        OKABE = 0x0F,
        BASEBALL_CAPTAIN = 0x10,
        GROCER = 0x11,
        GIRL = 0x12,
        OLD_LADY = 0x13,
        FAKE_HARUHI = 0x14,
        STRAY_CAT = 0x15,
        UNKNOWN = 0x16,
        INFO = 0x17,
        MONOLOGUE = 0x18,
        MAIL = 0x19,
    }
}
