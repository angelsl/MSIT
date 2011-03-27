// This file is part of MSIT. This file may have been taken from other applications and libraries.
// 
// MSIT is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// MSIT is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MSIT.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.IO;
using MapleLib.WzLib.Util;

namespace MapleLib.WzLib.WzProperties
{
    /// <summary>
    /// A property that contains data for an MP3 file
    /// </summary>
    public class WzSoundProperty : IExtended
    {
        #region Fields

        public static readonly byte[] soundHeaderMask = new byte[]
                                                            {
                                                                0x02, 0x83, 0xEB, 0x36, 0xE4, 0x4F, 0x52, 0xCE, 0x11, 0x9F, 0x53, 0x00, 0x20, 0xAF, 0x0B, 0xA7, 0x70, 0x8B, 0xEB, 0x36, 0xE4, 0x4F, 0x52,
                                                                0xCE, 0x11, 0x9F, 0x53, 0x00, 0x20, 0xAF, 0x0B, 0xA7, 0x70, 0x00, 0x01, 0x81, 0x9F, 0x58, 0x05, 0x56, 0xC3, 0xCE, 0x11, 0xBF, 0x01, 0x00,
                                                                0xAA, 0x00, 0x55, 0x59, 0x5A, 0x1E, 0x55, 0x00, 0x02, 0x00, /*FRQ 56*/0xAA, 0xBB, 0xCC, 0xDD /*/FRQ 59*/, 0x10, 0x27, 0x00, 0x00, 0x01,
                                                                0x00, 0x00, 0x00, 0x0C, 0x00, 0x01, 0x00, 0x02, 0x00, 0x00, 0x00, 0x0A, 0x02, 0x01, 0x00, 0x00, 0x00
                                                            };

        internal byte[] header;
        internal int len_ms;

        internal byte[] mp3bytes;
        internal string name;
        internal long offs;
        internal IWzObject parent;
        //internal WzImage imgParent;
        internal WzBinaryReader wzReader;

        #endregion

        /// <summary>
        /// Creates a WzSoundProperty with the specified name
        /// </summary>
        /// <param name="name">The name of the property</param>
        /// <param name="reader">The wz reader</param>
        /// <param name="parseNow">Indicating whether to parse the property now</param>
        public WzSoundProperty(string name, WzBinaryReader reader, bool parseNow)
        {
            this.name = name;
            wzReader = reader;
            reader.BaseStream.Position++;
            offs = reader.BaseStream.Position;
            //note - soundDataLen does NOT include the length of the header.
            int soundDataLen = reader.ReadCompressedInt();
            len_ms = reader.ReadCompressedInt();
            header = reader.ReadBytes(soundHeaderMask.Length);
            if (parseNow)
                mp3bytes = reader.ReadBytes(soundDataLen);
            else
                reader.BaseStream.Position += soundDataLen;
        }

        public WzSoundProperty(string name)
        {
            this.name = name;
            len_ms = 0;
            header = null;
            mp3bytes = null;
        }

        /// <summary>
        /// Creates a WzSoundProperty with the specified name and data
        /// </summary>
        /// <param name="name">The name of the property</param>
        /// <param name="len_ms">The sound length</param>
        /// <param name="header">The sound header</param>
        /// <param name="data">The sound data</param>
        public WzSoundProperty(string name, int len_ms, byte[] header, byte[] data)
        {
            this.name = name;
            this.len_ms = len_ms;
            this.header = header;
            mp3bytes = data;
        }

        /// <summary>
        /// Creates a WzSoundProperty with the specified name from a file
        /// </summary>
        /// <param name="name">The name of the property</param>
        /// <param name="file">The path to the sound file</param>
        public WzSoundProperty(string name, string file)
        {
            var header = new MP3Header();
            header.ReadMP3Information(file);
            len_ms = header.intLength*1000;
            byte[] frequencyBytes = BitConverter.GetBytes(header.intFrequency);
            var headerBytes = new byte[soundHeaderMask.Length];
            Array.Copy(soundHeaderMask, headerBytes, headerBytes.Length);
            for (int i = 0; i < 4; i++)
            {
                headerBytes[56 + i] = frequencyBytes[i];
            }
            this.header = headerBytes;
            mp3bytes = File.ReadAllBytes(file);
        }

        #region Parsing Methods

        public byte[] GetBytes(bool saveInMemory)
        {
            if (mp3bytes != null)
                return mp3bytes;
            else
            {
                if (wzReader == null) return null;
                long currentPos = wzReader.BaseStream.Position;
                wzReader.BaseStream.Position = offs;
                int soundDataLen = wzReader.ReadCompressedInt();
                wzReader.ReadCompressedInt();
                wzReader.BaseStream.Position += soundHeaderMask.Length;
                mp3bytes = wzReader.ReadBytes(soundDataLen);
                wzReader.BaseStream.Position = currentPos;
                if (saveInMemory)
                    return mp3bytes;
                else
                {
                    byte[] result = mp3bytes;
                    mp3bytes = null;
                    return result;
                }
            }
        }

        public void SetBytes(byte[] bytes)
        {
            mp3bytes = bytes;
        }

        public void SaveToFile(string file)
        {
            File.WriteAllBytes(file, GetBytes(false));
        }

        #endregion

        #region Cast Values

        internal override byte[] ToBytes(byte[] def)
        {
            return GetBytes(false);
        }

        #endregion

        public override object WzValue
        {
            get { return GetBytes(false); }
        }

        /// <summary>
        /// The parent of the object
        /// </summary>
        public override IWzObject Parent
        {
            get { return parent; }
            internal set { parent = value; }
        }

        /*/// <summary>
		/// The image that this property is contained in
		/// </summary>
		public override WzImage ParentImage { get { return imgParent; } internal set { imgParent = value; } }*/

        /// <summary>
        /// The name of the property
        /// </summary>
        public override string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// The WzPropertyType of the property
        /// </summary>
        public override WzPropertyType PropertyType
        {
            get { return WzPropertyType.Sound; }
        }

        /// <summary>
        /// The data of the mp3 header
        /// </summary>
        public byte[] Header
        {
            get { return header; }
            set { header = value; }
        }

        /// <summary>
        /// Length of the mp3 file in milliseconds
        /// </summary>
        public int Length
        {
            get { return len_ms; }
            set { len_ms = value; }
        }

        public override IWzImageProperty DeepClone()
        {
            var clone = (WzSoundProperty) MemberwiseClone();
            return clone;
        }

        public override void SetValue(object value)
        {
            if (value is byte[]) SetBytes((byte[]) value);
        }

        public override void WriteValue(WzBinaryWriter writer)
        {
            byte[] data = GetBytes(false);
            writer.WriteStringValue("Sound_DX8", 0x73, 0x1B);
            writer.Write((byte) 0);
            writer.WriteCompressedInt(data.Length);
            writer.WriteCompressedInt(len_ms);
            writer.Write(header);
            writer.Write(data);
        }

        public override void ExportXml(StreamWriter writer, int level)
        {
            writer.WriteLine(XmlUtil.Indentation(level) + XmlUtil.EmptyNamedTag("WzSound", Name));
        }

        /// <summary>
        /// Disposes the object
        /// </summary>
        public override void Dispose()
        {
            name = null;
            mp3bytes = null;
        }
    }
}