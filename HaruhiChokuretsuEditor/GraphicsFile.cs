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
        public byte[] CompressedData { get; set; }

        public void Initialize(byte[] decompressedData, int offset)
        {
            Offset = offset;
            Data = decompressedData.ToList();
            if (Encoding.ASCII.GetString(Data.Take(6).ToArray()) == "SHTXDS")
            {
                PaletteData = Data.Skip(0x14).Take(0x200).ToList();
                for (int i = 0; i < PaletteData.Count; i += 2)
                {
                    short color = BitConverter.ToInt16(PaletteData.Skip(i).Take(2).ToArray());
                    Palette.Add(Color.FromArgb((color & 0x1F) << 3, ((color >> 5) & 0x1F) << 3, ((color >> 10) & 0x1F) << 3));
                }

                while (Palette.Count < 256)
                {
                    Palette.Add(Color.FromArgb(0, 0, 0));
                }

                PixelData = Data.Skip(0x214).ToList();
            }
        }

        public void InitializeFontFile()
        {

        }

        public byte[] GetBytes() => Data.ToArray();

        public override string ToString()
        {
            return $"{Index:X3} 0x{Offset:X8}";
        }

        public Bitmap Get256ColorImage(int width = 256)
        {
            if (width % 8 != 0 || width > 256)
            {
                width = 256;
            }    
            int height = (256 / width) * 256;
            var bitmap = new Bitmap(width, height);
            int pixelIndex = 0;
            for (int row = 0; row < height / 8 && pixelIndex < PixelData.Count; row++)
            {
                for (int col = 0; col < width / 8 && pixelIndex < PixelData.Count; col++)
                {
                    for (int ypix = 0; ypix < 8 && pixelIndex < PixelData.Count; ypix++)
                    {
                        for (int xpix = 0; xpix < 8 && pixelIndex < PixelData.Count; xpix++)
                        {
                            bitmap.SetPixel((col << 3) + xpix, (row << 3) + ypix,
                                Palette[PixelData[pixelIndex++]]);
                        }
                    }
                }
            }
            return bitmap;
        }
    }
}
