using HaruhiChokuretsuEditor;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HaruhiChokuretsuTests
{
    public class GraphicsTests
    {
        [Test]
        // This file can be ripped directly from the ROM
        [TestCase(".\\inputs\\grp.bin")]
        public void GrpFileParserTest(string evtFile)
        {
            FileSystemFile<GraphicsFile> grp = FileSystemFile<GraphicsFile>.FromFile(evtFile);

            foreach (GraphicsFile graphicsFile in grp.Files)
            {
                Assert.AreEqual(graphicsFile.Offset, grp.RecalculateFileOffset(graphicsFile));
            }

            byte[] newGrpBytes = grp.GetBytes();
            Console.WriteLine($"Efficiency: {(double)newGrpBytes.Length / File.ReadAllBytes(evtFile).Length * 100}%");

            FileSystemFile<GraphicsFile> newGrpFile = new(newGrpBytes);
            Assert.AreEqual(newGrpBytes, newGrpFile.GetBytes());
        }
    }
}
