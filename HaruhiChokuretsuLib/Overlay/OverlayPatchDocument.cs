using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace HaruhiChokuretsuLib.Overlay
{
    public class OverlayPatchDocument
    {
        [XmlArray("overlays")]
        public OverlayXml[] Overlays { get; set; }
    }

    [XmlType("overlay")]
    public class OverlayXml
    {
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlElement("start")]
        public string start;
        [XmlIgnore]
        public uint Start { get => uint.Parse(start, System.Globalization.NumberStyles.HexNumber); set => start = $"{value:X8}"; }
        [XmlArray("patches")]
        public OverlayPatchXml[] Patches { get; set; }
    }
    
    [XmlType("replace")]
    public class OverlayPatchXml
    {
        [XmlAttribute("location")]
        public string location;
        [XmlAttribute("value")]
        public string patch;

        [XmlIgnore]
        public uint Location { get => uint.Parse(location, System.Globalization.NumberStyles.HexNumber); set => location = $"{value:X8}"; }
        [XmlIgnore]
        public byte[] Value { get => ByteArrayFromString(patch); set => patch = StringFromByteArray(value); }

        private byte[] ByteArrayFromString(string s)
        {
            if (s.Length % 2 != 0)
            {
                throw new ArgumentException($"Invalid string length {s.Length}; must be of even length");
            }
            List<byte> bytes = new();
            for (int i = 0; i < s.Length; i += 2)
            {
                bytes.Add(byte.Parse(s[i..(i + 2)], System.Globalization.NumberStyles.HexNumber));
            }
            return bytes.ToArray();
        }

        private string StringFromByteArray(byte[] ba)
        {
            string s = "";
            foreach (byte b in ba)
            {
                s += $"{b:X2}";
            }
            return s;
        }
    }
}
