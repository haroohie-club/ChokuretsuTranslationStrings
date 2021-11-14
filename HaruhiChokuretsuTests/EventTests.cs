using HaruhiChokuretsuLib;
using HaruhiChokuretsuLib.Archive;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HaruhiChokuretsuTests
{
    public class EventTests
    {
        [Test]
        [TestCase(TestVariables.EVT_000_DECOMPRESSED)]
        [TestCase(TestVariables.EVT_66_DECOMPRESSED)]
        [TestCase(TestVariables.EVT_MEMORYCARD_DECOMPRESSED)]
        [TestCase(TestVariables.EVT_TEST_DECOMPRESSED)]
        public void EventFileParserTest(string eventFile)
        {
            byte[] eventFileOnDisk = File.ReadAllBytes(eventFile);
            EventFile @event = new();
            @event.Initialize(eventFileOnDisk);

            Assert.AreEqual(eventFileOnDisk, @event.GetBytes());
        }

        [Test]
        [TestCase(TestVariables.EVT_000_DECOMPRESSED)]
        [TestCase(TestVariables.EVT_66_DECOMPRESSED)]
        [TestCase(TestVariables.EVT_MEMORYCARD_DECOMPRESSED)]
        [TestCase(TestVariables.EVT_TEST_DECOMPRESSED)]
        public void EventFileMovePointersIdempotentTest(string eventFile)
        {
            byte[] eventFileOnDisk = File.ReadAllBytes(eventFile);
            EventFile @event = new();
            @event.Initialize(eventFileOnDisk);

            string originalLine = @event.DialogueLines[0].Text;

            @event.EditDialogueLine(0, $"{originalLine}あ");
            @event.EditDialogueLine(0, $"{originalLine}");

            Assert.AreEqual(eventFileOnDisk, @event.GetBytes());
        }

        [Test]
        [TestCase(TestVariables.EVT_000_DECOMPRESSED, "evt_000")]
        [TestCase(TestVariables.EVT_66_DECOMPRESSED, "evt_66")]
        [TestCase(TestVariables.EVT_MEMORYCARD_DECOMPRESSED, "evt_memorycard")]
        [TestCase(TestVariables.EVT_TEST_DECOMPRESSED, "evt_test")]
        public void EventFileMovePointersTest(string eventFile, string prefix)
        {
            byte[] eventFileOnDisk = File.ReadAllBytes(eventFile);
            EventFile @event = new();
            @event.Initialize(eventFileOnDisk);

            List<int> allOldPointers = new();
            allOldPointers.Add(@event.PointerToNumEndPointers);
            allOldPointers.Add(@event.DialogueSectionPointer);
            allOldPointers.AddRange(@event.FrontPointers);
            allOldPointers.AddRange(@event.EndPointers);
            allOldPointers.AddRange(@event.EndPointerPointers);
            List<int> originalValues = new();
            foreach (int pointer in allOldPointers)
            {
                originalValues.Add(BitConverter.ToInt32(@event.Data.Skip(pointer).Take(4).ToArray()));
            }

            @event.EditDialogueLine(0, $"Ｃｈｅｅｋｉ　Ｂｒｅｅｋｉ　Ｈｏｍｉｅ");
            File.WriteAllBytes($".\\inputs\\{prefix}_edited.bin", @event.Data.ToArray());

            List<int> allNewPointers = new();
            allNewPointers.Add(@event.PointerToNumEndPointers);
            allNewPointers.Add(@event.DialogueSectionPointer);
            allNewPointers.AddRange(@event.FrontPointers);
            allNewPointers.AddRange(@event.EndPointers);
            allNewPointers.AddRange(@event.EndPointerPointers);
            List<int> newValues = new();
            foreach (int pointer in allNewPointers)
            {
                newValues.Add(BitConverter.ToInt32(@event.Data.Skip(pointer).Take(4).ToArray()));
            }

            Assert.AreEqual(originalValues, newValues);
        }

        [Test]
        // This file can be ripped directly from the ROM
        [TestCase(".\\inputs\\evt.bin")]
        public void EvtFileParserTest(string evtFile)
        {
            ArchiveFile<EventFile> evt = ArchiveFile<EventFile>.FromFile(evtFile);

            foreach (EventFile eventFile in evt.Files)
            {
                Assert.AreEqual(eventFile.Offset, evt.RecalculateFileOffset(eventFile));
            }

            byte[] newEvtBytes = evt.GetBytes();
            Console.WriteLine($"Efficiency: {(double)newEvtBytes.Length / File.ReadAllBytes(evtFile).Length * 100}%");

            ArchiveFile<EventFile> newEvtFile = new(newEvtBytes);
            Assert.AreEqual(evt.Files.Count, newEvtFile.Files.Count);
            for (int i = 0; i < newEvtFile.Files.Count; i++)
            {
                Assert.AreEqual(evt.Files[i].Data, newEvtFile.Files[i].Data, $"Failed at file {i} (offset: 0x{evt.Files[i].Offset:X8}; index: {evt.Files[i].Index:X4}");
            }

            Assert.AreEqual(newEvtBytes, newEvtFile.GetBytes());
        }
    }
}
