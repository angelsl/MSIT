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

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using MSIT.WzLib.Util;

namespace MSIT.WzLib.WzProperties
{
    /// <summary>
    ///   A property that contains the information for a bitmap
    /// </summary>
    public class WzPngProperty : IWzImageProperty
    {
        #region Fields

        private readonly int _format;
        private readonly int _format2;

        private readonly long _offset;
        private readonly WzBinaryReader _wzReader;
        private byte[] _compressedBytes;
        private int _height;
        private bool _listWzUsed;
        private Bitmap _png;
        private int _width;

        #endregion

        /// <summary>
        ///   Creates a blank WzPngProperty
        /// </summary>
        internal WzPngProperty(WzBinaryReader reader, bool parseNow)
        {
            // Read compressed bytes
            _width = reader.ReadCompressedInt();
            _height = reader.ReadCompressedInt();
            _format = reader.ReadCompressedInt();
            _format2 = reader.ReadByte();
            reader.BaseStream.Position += 4;
            _offset = reader.BaseStream.Position;
            int len = reader.ReadInt32() - 1;
            reader.BaseStream.Position += 1;

            if (len > 0)
            {
                if (parseNow)
                {
                    _compressedBytes = _wzReader.ReadBytes(len);
                    ParsePng();
                }
                else reader.BaseStream.Position += len;
            }
            _wzReader = reader;
        }

        #region Parsing Methods

        private byte[] GetCompressedBytes(bool saveInMemory)
        {
            if (_compressedBytes == null)
            {
                long pos = _wzReader.BaseStream.Position;
                _wzReader.BaseStream.Position = _offset;
                int len = _wzReader.ReadInt32() - 1;
                _wzReader.BaseStream.Position += 1;
                if (len > 0) _compressedBytes = _wzReader.ReadBytes(len);
                _wzReader.BaseStream.Position = pos;
                if (!saveInMemory)
                {
                    //were removing the referance to compressedBytes, so a backup for the ret value is needed
                    byte[] returnBytes = _compressedBytes;
                    _compressedBytes = null;
                    return returnBytes;
                }
            }
            return _compressedBytes;
        }


        public Bitmap GetPng(bool saveInMemory)
        {
            if (_png == null)
            {
                long pos = _wzReader.BaseStream.Position;
                _wzReader.BaseStream.Position = _offset;
                int len = _wzReader.ReadInt32() - 1;
                _wzReader.BaseStream.Position += 1;
                if (len > 0) _compressedBytes = _wzReader.ReadBytes(len);
                ParsePng();
                _wzReader.BaseStream.Position = pos;
                if (!saveInMemory)
                {
                    Bitmap pngImage = _png;
                    _png = null;
                    _compressedBytes = null;
                    return pngImage;
                }
            }
            return _png;
        }

        private void ParsePng()
        {
            DeflateStream zlib;
            int uncompressedSize;
            int x = 0, y = 0;
            Bitmap bmp = null;
            BitmapData bmpData;
            WzImage imgParent = ParentImage;
            byte[] decBuf;

            BinaryReader reader = new BinaryReader(new MemoryStream(_compressedBytes));
            ushort header = reader.ReadUInt16();
            _listWzUsed = header != 0x9C78 && header != 0xDA78;
            if (!_listWzUsed)
            {
                zlib = new DeflateStream(reader.BaseStream, CompressionMode.Decompress);
            }
            else
            {
                reader.BaseStream.Position -= 2;
                MemoryStream dataStream = new MemoryStream();
                int endOfPng = _compressedBytes.Length;

                while (reader.BaseStream.Position < endOfPng)
                {
                    int blocksize = reader.ReadInt32();
                    for (int i = 0; i < blocksize; i++)
                    {
                        dataStream.WriteByte((byte) (reader.ReadByte() ^ imgParent.reader.WzKey[i]));
                    }
                }
                dataStream.Position = 2;
                zlib = new DeflateStream(dataStream, CompressionMode.Decompress);
            }

            switch (_format + _format2)
            {
                case 1:
                    bmp = new Bitmap(_width, _height, PixelFormat.Format32bppArgb);
                    bmpData = bmp.LockBits(new Rectangle(0, 0, _width, _height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                    uncompressedSize = _width*_height*2;
                    decBuf = new byte[uncompressedSize];
                    zlib.Read(decBuf, 0, uncompressedSize);
                    byte[] argb = new Byte[uncompressedSize*2];
                    for (int i = 0; i < uncompressedSize; i++)
                    {
                        int b = decBuf[i] & 0x0F;
                        b |= (b << 4);
                        argb[i*2] = (byte) b;
                        int g = decBuf[i] & 0xF0;
                        g |= (g >> 4);
                        argb[i*2 + 1] = (byte) g;
                    }
                    Marshal.Copy(argb, 0, bmpData.Scan0, argb.Length);
                    bmp.UnlockBits(bmpData);
                    break;
                case 2:
                    bmp = new Bitmap(_width, _height, PixelFormat.Format32bppArgb);
                    bmpData = bmp.LockBits(new Rectangle(0, 0, _width, _height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                    uncompressedSize = _width*_height*4;
                    decBuf = new byte[uncompressedSize];
                    zlib.Read(decBuf, 0, uncompressedSize);
                    Marshal.Copy(decBuf, 0, bmpData.Scan0, decBuf.Length);
                    bmp.UnlockBits(bmpData);
                    break;
                case 513:
                    bmp = new Bitmap(_width, _height, PixelFormat.Format16bppRgb565);
                    bmpData = bmp.LockBits(new Rectangle(0, 0, _width, _height), ImageLockMode.WriteOnly, PixelFormat.Format16bppRgb565);
                    uncompressedSize = _width*_height*2;
                    decBuf = new byte[uncompressedSize];
                    zlib.Read(decBuf, 0, uncompressedSize);
                    Marshal.Copy(decBuf, 0, bmpData.Scan0, decBuf.Length);
                    bmp.UnlockBits(bmpData);
                    break;

                case 517:
                    bmp = new Bitmap(_width, _height);
                    uncompressedSize = _width*_height/128;
                    decBuf = new byte[uncompressedSize];
                    zlib.Read(decBuf, 0, uncompressedSize);
                    for (int i = 0; i < uncompressedSize; i++)
                    {
                        for (byte j = 0; j < 8; j++)
                        {
                            byte iB = Convert.ToByte(((decBuf[i] & (0x01 << (7 - j))) >> (7 - j))*0xFF);
                            for (int k = 0; k < 16; k++)
                            {
                                if (x == _width)
                                {
                                    x = 0;
                                    y++;
                                }
                                bmp.SetPixel(x, y, Color.FromArgb(0xFF, iB, iB, iB));
                                x++;
                            }
                        }
                    }
                    break;
            }
            _png = bmp;
        }

        #endregion

        #region Cast Values

        internal override WzPngProperty ToPngProperty(WzPngProperty def)
        {
            return this;
        }

        internal override Bitmap ToBitmap(Bitmap def)
        {
            return GetPng(false);
        }

        internal override byte[] ToBytes(byte[] def)
        {
            return base.ToBytes(def);
        }

        #endregion

        public override object WzValue
        {
            get { return GetPng(false); }
        }

        /// <summary>
        ///   The parent of the object
        /// </summary>
        public override IWzObject Parent { get; internal set; }

        /*/// <summary>
        /// The image that this property is contained in
        /// </summary>
        public override WzImage ParentImage { get { return imgParent; } internal set { imgParent = value; } }*/

        /// <summary>
        ///   The name of the property
        /// </summary>
        public override string Name
        {
            get { return "PNG"; }
        }

        /// <summary>
        ///   The WzPropertyType of the property
        /// </summary>
        public override WzPropertyType PropertyType
        {
            get { return WzPropertyType.PNG; }
        }

        /// <summary>
        ///   The width of the bitmap
        /// </summary>
        public int Width
        {
            get { return _width; }
            set { _width = value; }
        }

        /// <summary>
        ///   The height of the bitmap
        /// </summary>
        public int Height
        {
            get { return _height; }
            set { _height = value; }
        }

        public override IWzImageProperty DeepClone()
        {
            WzPngProperty clone = (WzPngProperty) MemberwiseClone();
            clone._compressedBytes = GetCompressedBytes(false);
            return clone;
        }

        /// <summary>
        ///   Disposes the object
        /// </summary>
        public override void Dispose()
        {
            _compressedBytes = null;
            if (_png != null)
            {
                _png.Dispose();
                _png = null;
            }
        }
    }
}