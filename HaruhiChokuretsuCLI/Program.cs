using HaruhiChokuretsuLib;
using HaruhiChokuretsuLib.Archive;
using System;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;

namespace HaruhiChokuretsuCLI
{
    class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                ShowHelp();
                return;
            }

            var mode = args[0];
            var inPath = args[1];
            var outPath = args[2];

            if (mode == "-u" || mode == "--unpack")
            {
                UnpackAll(inPath, outPath);
            }
            else if (mode == "-x" || mode == "--extract")
            {
                if (args.Length == 4)
                    ExtractSingle(inPath, outPath, int.Parse(args[3]));
                else
                    ExtractSingle(inPath, outPath);
            }
            else if ((mode == "-r" || mode == "--replace") && args.Length == 4)
            {
                var inputFolder = args[3];

                ReplaceFromFolder(inputFolder, inPath, outPath);
            }
            else
            {
                ShowHelp();
            }
        }

        private static void ShowHelp()
        {
            //todo
        }

        private static int? GetIndexByFileName(string filePath)
        {
            try
            {
                var fileInfo = new FileInfo(filePath);
                var fileName = fileInfo.Name.Replace(fileInfo.Extension, "");

                if (fileName.Contains("ignore"))
                    return null;

                return int.Parse(fileName.Split('_')[0], NumberStyles.HexNumber);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Unpacks all files (compressed)
        /// </summary>
        /// <param name="inPath"></param>
        /// <param name="outPath"></param>
        private static void UnpackAll(string inPath, string outPath)
        {
            var name = new FileInfo(inPath).Name;

            if (!Directory.Exists(outPath))
                Directory.CreateDirectory(outPath);

            var archive = ArchiveFile<FileInArchive>.FromFile(inPath);

            archive.Files.ForEach(x => File.WriteAllBytes(Path.Combine(outPath, $"{x.Index:X3}.bin"), x.CompressedData));
        }

        /// <summary>
        /// Extract a single file from the archive
        /// </summary>
        /// <param name="inputArc"></param>
        /// <param name="outputFile"></param>
        /// <param name="width"></param>
        private static void ExtractSingle(string inputArc, string outputFile, int width = 256)
        {
            var inputName = new FileInfo(inputArc).Name;

            var outputFileInfo = new FileInfo(outputFile);
            var index = int.Parse(outputFileInfo.Name.Replace(outputFileInfo.Extension, ""), NumberStyles.HexNumber);

            var outFolder = outputFileInfo.DirectoryName;

            if (!Directory.Exists(outFolder))
                Directory.CreateDirectory(outFolder);

            try
            {
                if (inputName.ToLower().StartsWith("grp"))
                {
                    var grpArc = ArchiveFile<GraphicsFile>.FromFile(inputArc);

                    var file = grpArc.Files.FirstOrDefault(x => x.Index == index);

                    if (index == 0xE50)
                        file.InitializeFontFile();

                    file.GetImage(width).Save(outputFile, ImageFormat.Png);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Cannot extract file from {inputName} #{index:X3}");
            }
        }

        /// <summary>
        /// Replace graphics file by converting and compressing it too
        /// </summary>
        /// <param name="arc"></param>
        /// <param name="filePath"></param>
        /// <param name="index"></param>
        private static void ReplaceSingleGraphicFile(ArchiveFile<FileInArchive> arc, string filePath, int index)
        {
            var file = arc.Files.FirstOrDefault(x => x.Index == index);

            var decompressedData = Helpers.DecompressData(file.CompressedData);

            var grpFile = new GraphicsFile();
            grpFile.Initialize(decompressedData, file.Offset);
            grpFile.Index = index;

            if (index == 0xE50)
                grpFile.InitializeFontFile();

            grpFile.SetImage(filePath);

            file.CompressedData = Helpers.CompressData(grpFile.GetBytes());
        }

        /// <summary>
        /// Replace any compressed file type
        /// </summary>
        /// <param name="arc"></param>
        /// <param name="filePath"></param>
        /// <param name="index"></param>
        private static void ReplaceSingleFile(ArchiveFile<FileInArchive> arc, string filePath, int index)
        {
            var file = arc.Files.FirstOrDefault(x => x.Index == index);
            file.CompressedData = File.ReadAllBytes(filePath);
        }

        /// <summary>
        /// Replace file without decompressing the others first
        /// </summary>
        /// <param name="inputFolder"></param>
        /// <param name="inputArc"></param>
        /// <param name="outputArc"></param>
        private static void ReplaceFromFolder(string inputFolder, string inputArc, string outputArc)
        {
            var inputName = new FileInfo(inputArc).Name;

            try
            {
                var arc = ArchiveFile<FileInArchive>.FromFile(inputArc);
                var files = Directory.EnumerateFiles(inputFolder);

                foreach (var filePath in files)
                {
                    var index = GetIndexByFileName(filePath);

                    if (index.HasValue)
                    {
                        Console.Write($"Replacing #{index:X3}... ");

                        try
                        {
                            if (inputName.ToLower().StartsWith("grp"))
                                ReplaceSingleGraphicFile(arc, filePath, index.Value);
                            else
                                ReplaceSingleFile(arc, filePath, index.Value);

                            Console.WriteLine("OK");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"NOT OK: {e.Message}");
                        }
                    }
                }

                File.WriteAllBytes(outputArc, arc.GetBytes());
            }
            catch (Exception e)
            {
                Console.WriteLine($"Fatal error: {e.Message}");
            }
        }
    }
}
