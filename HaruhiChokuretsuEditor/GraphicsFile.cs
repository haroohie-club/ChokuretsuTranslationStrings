using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaruhiChokuretsuEditor
{
    public class GraphicsFile : IFile
    {
        public int Index { get; set; }
        public int Offset { get; set; }
        public List<byte> Data { get; set; }
        public List<byte> PaletteData { get; set; }
        public List<Color> Palette { get; set; } = new();
        public List<byte> PixelData { get; set; }
        public TileForm ImageTileForm { get; set; }
        public byte[] CompressedData { get; set; }
        public bool Shtxds { get; set; } = false;

        private readonly static int[] VALID_WIDTHS = new int[] { 8, 16, 32, 64, 128, 256 };

        public enum TileForm
        {
            // corresponds to number of colors
            GBA_4BPP = 0x10,
            GBA_8BPP = 0x100,
        }

        public void Initialize(byte[] decompressedData, int offset)
        {
            Offset = offset;
            Data = decompressedData.ToList();
            if (Encoding.ASCII.GetString(Data.Take(6).ToArray()) == "SHTXDS")
            {
                Shtxds = true;
                ImageTileForm = (TileForm)(BitConverter.ToInt16(decompressedData.Skip(0x06).Take(2).ToArray()));

                int paletteLength = 0x200;
                if (ImageTileForm == TileForm.GBA_4BPP)
                {
                    paletteLength = 0x60;
                }

                PaletteData = Data.Skip(0x14).Take(paletteLength).ToList();
                for (int i = 0; i < PaletteData.Count; i += 2)
                {
                    short color = BitConverter.ToInt16(PaletteData.Skip(i).Take(2).ToArray());
                    Palette.Add(Color.FromArgb((color & 0x1F) << 3, ((color >> 5) & 0x1F) << 3, ((color >> 10) & 0x1F) << 3));
                }

                while (Palette.Count < 256)
                {
                    Palette.Add(Color.FromArgb(0, 0, 0));
                }

                PixelData = Data.Skip(paletteLength + 0x14).ToList();
            }
        }

        public void InitializeFontFile()
        {
            ImageTileForm = TileForm.GBA_4BPP;
            // grayscale palette
            Palette = new()
            {
                Color.FromArgb(0x00, 0x00, 0x00),
                Color.FromArgb(0x0F, 0x0F, 0x0F),
                Color.FromArgb(0x2F, 0x2F, 0x2F),
                Color.FromArgb(0x3F, 0x3F, 0x3F),
                Color.FromArgb(0x4F, 0x4F, 0x4F),
                Color.FromArgb(0x4F, 0x4F, 0x4F),
                Color.FromArgb(0x5F, 0x5F, 0x5F),
                Color.FromArgb(0x6F, 0x6F, 0x6F),
                Color.FromArgb(0x7F, 0x7F, 0x7F),
                Color.FromArgb(0x8F, 0x8F, 0x8F),
                Color.FromArgb(0x9F, 0x9F, 0x9F),
                Color.FromArgb(0xAF, 0xAF, 0xAF),
                Color.FromArgb(0xBF, 0xBF, 0xBF),
                Color.FromArgb(0xCF, 0xCF, 0xCF),
                Color.FromArgb(0xDF, 0xDF, 0xDF),
                Color.FromArgb(0xEF, 0xEF, 0xEF),
                Color.FromArgb(0xFF, 0xFF, 0xFF),
            };
            PixelData = Data;
        }

        public byte[] GetBytes()
        {
            if (Shtxds)
            {
                List<byte> data = new();
                data.AddRange(Data.Take(0x14)); // get header
                data.AddRange(PaletteData);
                data.AddRange(PixelData);

                return data.ToArray();
            }
            else if (Index == 0xE50) // more special casing for the font file
            {
                return PixelData.ToArray();
            }
            else
            {
                return Data.ToArray();
            }
        }

        public override string ToString()
        {
            return $"{Index:X3} 0x{Offset:X8}";
        }

        public Bitmap GetImage(int width = 256)
        {
            if (!VALID_WIDTHS.Contains(width))
            {
                width = 256;
            }
            int height = 4 * (PixelData.Count / (ImageTileForm == TileForm.GBA_4BPP ? 32 : 64));
            var bitmap = new Bitmap(width, height);
            int pixelIndex = 0;
            for (int row = 0; row < height / 8 && pixelIndex < PixelData.Count; row++)
            {
                for (int col = 0; col < width / 8 && pixelIndex < PixelData.Count; col++)
                {
                    for (int ypix = 0; ypix < 8 && pixelIndex < PixelData.Count; ypix++)
                    {
                        if (ImageTileForm == TileForm.GBA_4BPP)
                        {
                            for (int xpix = 0; xpix < 4 && pixelIndex < PixelData.Count; xpix++)
                            {
                                for (int xypix = 0; xypix < 2 && pixelIndex < PixelData.Count; xypix++)
                                {
                                    bitmap.SetPixel((col * 8) + (xpix * 2) + xypix, (row * 8) + ypix,
                                        Palette[PixelData[pixelIndex] >> (xypix * 4) & 0xF]);
                                }
                                pixelIndex++;
                            }
                        }
                        else
                        {
                            for (int xpix = 0; xpix < 8 && pixelIndex < PixelData.Count; xpix++)
                            {
                                bitmap.SetPixel((col * 8) + xpix, (row * 8) + ypix,
                                    Palette[PixelData[pixelIndex++]]);
                            }
                        }
                    }
                }
            }
            return bitmap;
        }

        /// <summary>
        /// Replaces the current pixel data with a bitmap image on disk
        /// </summary>
        /// <param name="bitmapFile">Path to bitmap file to import</param>
        /// <returns>Width of new bitmap image</returns>
        public int SetImage(string bitmapFile)
        {
            return SetImage(new Bitmap(bitmapFile));
        }

        /// <summary>
        /// Replaces the current pixel data with a bitmap image in memory
        /// </summary>
        /// <param name="bitmap">Bitmap image in memory</param>
        /// <returns>Width of new bitmap image</returns>
        public int SetImage(Bitmap bitmap)
        {
            if (!VALID_WIDTHS.Contains(bitmap.Width))
            {
                throw new ArgumentException($"Image width {bitmap.Width} not a valid width.");
            }
            int calculatedHeight = 4 * (PixelData.Count / (ImageTileForm == TileForm.GBA_4BPP ? 32 : 64));
            if (bitmap.Height != calculatedHeight)
            {
                throw new ArgumentException($"Image height {bitmap.Height} does not match calculated height {calculatedHeight}.");
            }

            List<byte> pixelData = new();

            for (int row = 0; row < bitmap.Height / 8 && pixelData.Count < PixelData.Count; row++)
            {
                for (int col = 0; col < bitmap.Width / 8 && pixelData.Count < PixelData.Count; col++)
                {
                    for (int ypix = 0; ypix < 8 && pixelData.Count < PixelData.Count; ypix++)
                    {
                        if (ImageTileForm == TileForm.GBA_4BPP)
                        {
                            for (int xpix = 0; xpix < 4 && pixelData.Count < PixelData.Count; xpix++)
                            {
                                int color1 = Helpers.ClosestColorIndex(Palette, bitmap.GetPixel((col * 8) + (xpix * 2), (row * 8) + ypix));
                                int color2 = Helpers.ClosestColorIndex(Palette, bitmap.GetPixel((col * 8) + (xpix * 2) + 1, (row * 8) + ypix));

                                pixelData.Add((byte)(color1 + (color2 << 4)));
                            }
                        }
                        else
                        {
                            for (int xpix = 0; xpix < 8 && pixelData.Count < PixelData.Count; xpix++)
                            {
                                pixelData.Add((byte)Helpers.ClosestColorIndex(Palette, bitmap.GetPixel((col * 8) + xpix, (row * 8) + ypix)));
                            }
                        }
                    }
                }
            }
            PixelData = pixelData;
            return bitmap.Width;
        }
    }
}
