using HaruhiChokuretsuEditor;
using NUnit.Framework;
using System.IO;

namespace HaruhiChokuretsuTests
{
    public class CompressionTests
    {
        [Test]
        [TestCase("evt_000", TestVariables.EVT_000_COMPRESSED, TestVariables.EVT_000_DECOMPRESSED)]
        [TestCase("grp_test", TestVariables.GRP_TEST_COMPRESSED, TestVariables.GRP_TEST_DECOMPRESSED)]
        public void AsmSimulatorTest(string filePrefix, string compressedFile, string decompressedFile)
        {
            byte[] compressedData = File.ReadAllBytes(compressedFile);
            AsmDecompressionSimulator asm = new(compressedData);

            byte[] decompressedDataOnDisk = File.ReadAllBytes(decompressedFile);
            File.WriteAllBytes($".\\inputs\\{filePrefix}_asm_decomp.bin", asm.Output);
            Assert.AreEqual(decompressedDataOnDisk, asm.Output);
        }

        [Test]
        [TestCase("evt_000", TestVariables.EVT_000_COMPRESSED, TestVariables.EVT_000_DECOMPRESSED)]
        [TestCase("grp_test", TestVariables.GRP_TEST_COMPRESSED, TestVariables.GRP_TEST_DECOMPRESSED)]
        public void DecompressionMethodTest(string filePrefix, string compressedFile, string decompressedFile)
        {
            byte[] decompressedDataInMemory = Helpers.DecompressData(File.ReadAllBytes(compressedFile));
            File.WriteAllBytes($".\\inputs\\{filePrefix}_prog_decomp.bin", decompressedDataInMemory);

            byte[] decompressedDataOnDisk = File.ReadAllBytes(decompressedFile);
            Assert.AreEqual(decompressedDataOnDisk, decompressedDataInMemory);
        }

        [Test]
        [TestCase("evt_000", TestVariables.EVT_000_DECOMPRESSED)]
        [TestCase("grp_test", TestVariables.GRP_TEST_COMPRESSED)]
        public void CompressionMethodTest(string filePrefix, string decompressedFile)
        {
            byte[] decompressedDataOnDisk = File.ReadAllBytes(decompressedFile);
            byte[] compressedData = Helpers.CompressData(decompressedDataOnDisk);
            File.WriteAllBytes($".\\inputs\\{filePrefix}_prog_comp.bin", compressedData);
            byte[] decompressedDataInMemory = Helpers.DecompressData(compressedData);
            File.WriteAllBytes($".\\inputs\\{filePrefix}_prog_decomp.bin", decompressedDataInMemory);
            byte[] decompressedDataViaAsm = new AsmDecompressionSimulator(compressedData).Output;
            File.WriteAllBytes($".\\inputs\\{filePrefix}_asm_decomp.bin", decompressedDataViaAsm);

            Assert.AreEqual(decompressedDataOnDisk, decompressedDataInMemory, message: "Failed in implementation.");
            Assert.AreEqual(decompressedDataOnDisk, decompressedDataViaAsm, message: "Failed in assembly simulation.");
        }
    }
}