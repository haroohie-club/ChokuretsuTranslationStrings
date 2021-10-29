using HaruhiChokuretsuEditor;
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
        public void EventFileParserTest(string eventFile)
        {
            byte[] eventFileOnDisk = File.ReadAllBytes(eventFile);
            EventFile @event = new(eventFileOnDisk);

            Assert.AreEqual(eventFileOnDisk, @event.GetBytes());
        }

        [Test]
        // This file can be ripped directly from the ROM
        [TestCase(".\\inputs\\evt.bin")]
        public void EvtFileParserTest(string evtFile)
        {
            EvtFile evt = EvtFile.FromFile(evtFile, out _);;

            foreach (EventFile eventFile in evt.EventFiles)
            {
                Assert.AreEqual(eventFile.Offset, evt.RecalculateEventOffset(eventFile));
            }

            byte[] newEvtBytes = evt.GetBytes();
            Console.WriteLine($"Efficiency: {(double)newEvtBytes.Length / File.ReadAllBytes(evtFile).Length * 100}%");

            EvtFile newEvtFile = new EvtFile(newEvtBytes, out _);
            Assert.AreEqual(newEvtBytes, newEvtFile.GetBytes());
        }
    }
}
