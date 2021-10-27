using HaruhiChokuretsuEditor;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

namespace HaruhiChokuretsuTests
{
    public class EventTests
    {
        [Test]
        [TestCase(TestVariables.EVT_000_DECOMPRESSED)]
        public void EventFileParserTest(string eventFile)
        {
            EventFile @event = new(File.ReadAllBytes(eventFile));
        }
    }
}
