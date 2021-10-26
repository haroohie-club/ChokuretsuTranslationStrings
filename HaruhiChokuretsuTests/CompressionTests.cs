using HaruhiChokuretsuEditor;
using NUnit.Framework;
using System.IO;

namespace HaruhiChokuretsuTests
{
    public class CompressionTests
    {
        [Test]
        [TestCase(TestVariables.EVT_000_COMPRESSED, TestVariables.EVT_000_DECOMPRESSED)]
        public void AsmSimulatorTest(string compressedFile, string decompressedFile)
        {
            byte[] compressedData = File.ReadAllBytes(compressedFile);
            AsmDecompressionSimulator asm = new(compressedData);

            byte[] decompressedDataOnDisk = File.ReadAllBytes(decompressedFile);
            File.WriteAllBytes(".\\inputs\\ev_000_asm_decomp.bin", asm.Output);
            Assert.AreEqual(decompressedDataOnDisk, asm.Output);
        }

        [Test]
        [TestCase(TestVariables.EVT_000_COMPRESSED, TestVariables.EVT_000_DECOMPRESSED)]
        public void DecompressionMethodTest(string compressedFile, string decompressedFile)
        {
            byte[] decompressedDataInMemory = Helpers.DecompressData(File.ReadAllBytes(compressedFile));
            File.WriteAllBytes(".\\inputs\\ev_000_prog_decomp.bin", decompressedDataInMemory);

            byte[] decompressedDataOnDisk = File.ReadAllBytes(decompressedFile);
            Assert.AreEqual(decompressedDataOnDisk, decompressedDataInMemory);
        }

        [Test]
        [TestCase(TestVariables.EVT_000_DECOMPRESSED)]
        public void CompressionMethodTest(string decompressedFile)
        {
            byte[] decompressedDataOnDisk = File.ReadAllBytes(decompressedFile);
            byte[] compressedData = Helpers.CompressData(decompressedDataOnDisk);
            File.WriteAllBytes(".\\inputs\\ev_000_prog_comp.bin", compressedData);
            byte[] decompressedDataInMemory = Helpers.DecompressData(compressedData);
            File.WriteAllBytes(".\\inputs\\ev_000_prog_decomp.bin", decompressedDataInMemory);

            Assert.AreEqual(decompressedDataOnDisk, decompressedDataInMemory);
        }
    }
}