// This file is part of MSIT.
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

using System.IO;
using System.Text;

namespace MSIT.WzLib.Util
{
    public class WzBinaryReader : BinaryReader
    {
        #region Properties

        public byte[] WzKey { get; set; }
        public uint Hash { get; set; }
        public WzHeader Header { get; set; }

        #endregion

        #region Constructors

        public WzBinaryReader(Stream input, byte[] WzIv) : base(input)
        {
            WzKey = WzKeyGenerator.GenerateWzKey(WzIv);
        }

        #endregion

        #region Methods

        public string ReadStringAtOffset(long Offset)
        {
            return ReadStringAtOffset(Offset, false);
        }

        public string ReadStringAtOffset(long Offset, bool readByte, bool enc = true)
        {
            long CurrentOffset = BaseStream.Position;
            BaseStream.Position = Offset;
            if (readByte)
            {
                ReadByte();
            }
            string ReturnString = ReadWzString(enc);
            BaseStream.Position = CurrentOffset;
            return ReturnString;
        }

        public override string ReadString()
        {
            return ReadWzString();
        }

        public string ReadWzString(bool enc = true)
        {
            sbyte smallLength = base.ReadSByte();

            if (smallLength == 0)
            {
                return string.Empty;
            }

            int length;
            StringBuilder retString = new StringBuilder();
            if (smallLength > 0) // Unicode
            {
                ushort mask = 0xAAAA;
                length = smallLength == sbyte.MaxValue ? ReadInt32() : smallLength;
                if (length > 0)
                {
                    for (int i = 0; i < length; i++)
                    {
                        ushort encryptedChar = ReadUInt16();
                        encryptedChar ^= mask;
                        if (enc)
                        {
                            encryptedChar ^= (ushort) ((WzKey[i*2 + 1] << 8) + WzKey[i*2]);
                        }
                        retString.Append((char) encryptedChar);
                        mask++;
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
            else
            {
                // ASCII
                byte mask = 0xAA;
                length = smallLength == sbyte.MinValue ? ReadInt32() : -smallLength;
                if (length > 0)
                {
                    for (int i = 0; i < length; i++)
                    {
                        byte encryptedChar = ReadByte();
                        encryptedChar ^= mask;
                        if (enc)
                        {
                            encryptedChar ^= WzKey[i];
                        }
                        retString.Append((char) encryptedChar);
                        mask++;
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
            return retString.ToString();
        }

        /// <summary>
        ///   Reads an ASCII string, without decryption
        /// </summary>
        /// <param name="filePath"> Length of bytes to read </param>
        public string ReadString(int length)
        {
            return Encoding.ASCII.GetString(ReadBytes(length));
        }

        public string ReadNullTerminatedString()
        {
            StringBuilder retString = new StringBuilder();
            byte b = ReadByte();
            while (b != 0)
            {
                retString.Append((char) b);
                b = ReadByte();
            }
            return retString.ToString();
        }

        public int ReadCompressedInt()
        {
            sbyte sb = base.ReadSByte();
            if (sb == sbyte.MinValue)
            {
                return ReadInt32();
            }
            return sb;
        }

        public uint ReadOffset()
        {
            uint offset = (uint) BaseStream.Position;
            offset = (offset - Header.FStart) ^ uint.MaxValue;
            offset *= Hash;
            offset -= CryptoConstants.WZOffsetConstant;
            offset = WzTool.RotateLeft(offset, (byte) (offset & 0x1F));
            uint encryptedOffset = ReadUInt32();
            offset ^= encryptedOffset;
            offset += Header.FStart*2;
            return offset;
        }

        public string ReadStringBlock(uint offset, bool enc = true)
        {
            switch (ReadByte())
            {
                case 0:
                case 0x73:
                    return ReadWzString(enc);
                case 1:
                case 0x1B:
                    return ReadStringAtOffset(offset + ReadInt32(), false, enc);
                default:
                    return "";
            }
        }

        #endregion
    }
}