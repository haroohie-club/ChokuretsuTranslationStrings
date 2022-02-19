using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaruhiChokuretsuLib.Overlay
{
    public class Overlay
    {
        public string Name { get; set; }
        public List<byte> Data { get; set; }

        public Overlay(string file)
        {
            Name = Path.GetFileNameWithoutExtension(file);
            Data = File.ReadAllBytes(file).ToList();
        }

        public void Save(string file)
        {
            File.WriteAllBytes(file, Data.ToArray());
        }

        public void Patch(int loc, byte[] patchData)
        {
            Data.RemoveRange(loc, patchData.Length);
            Data.InsertRange(loc, patchData);
        }
    }
}
