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
using System.Text;

#region .NET Disclaimer/Info

//===============================================================================
//
// gOODiDEA, uland.com
//===============================================================================
//
// $Header :		$  
// $Author :		$
// $Date   :		$
// $Revision:		$
// $History:		$  
//  
//===============================================================================

#endregion

#region Java

/**
 * Class GifEncoder - Encodes a GIF file consisting of one or
 * more frames.
 * <pre>
 * Example:
 *    GifEncoder e = new GifEncoder();
 *    e.start(outputFileName);
 *    e.setDelay(1000);   // 1 frame per sec
 *    e.addFrame(image1);
 *    e.addFrame(image2);
 *    e.finish();
 * </pre>
 * No copyright asserted on the source code of this class.  May be used
 * for any purpose, however, refer to the Unisys LZW patent for restrictions
 * on use of the associated LZWEncoder class.  Please forward any corrections
 * to kweiner@fmsware.com.
 *
 * @author Kevin Weiner, FM Software
 * @version 1.03 November 2003
 *
 */

#endregion

namespace MSIT.NGif
{
    public class GifEncoder
    {
        private bool _closeStream; // close stream when finished
        private int _colorDepth; // number of bit planes
        private byte[] _colorTab; // RGB palette
        private int _delay; // frame delay (hundredths)
        private int _dispose = -1; // disposal code (-1 = use default)
        private bool _firstFrame = true;
        private FileStream _fs;
        private int _height;

        private Image _image; // current frame
        private byte[] _indexedPixels; // converted frame indexed to palette
        private int _palSize = 7; // color table size (bits-1)
        private byte[] _pixels; // BGR byte array from frame
        private int _repeat = -1; // no repeat
        private int _sample = 10; // default sample interval for quantizer
        private bool _sizeSet; // if false, get size from first frame
        private bool _started; // ready to output frames
        private int _transIndex; // transparent index in color table
        private Color _transparent = Color.Empty; // transparent color if given
        private readonly bool[] _usedEntry = new bool[256]; // active palette entries
        private int _width; // image size

        /**
		 * Sets the delay time between each frame, or changes it
		 * for subsequent frames (applies to last frame added).
		 *
		 * @param ms int delay time in milliseconds
		 */

        public void SetDelay(int ms)
        {
            _delay = ms/10;
        }

        /**
		 * Sets the GIF frame disposal code for the last added frame
		 * and any subsequent frames.  Default is 0 if no transparent
		 * color has been set, otherwise 2.
		 * @param code int disposal code.
		 */

        public void SetDispose(int code)
        {
            if (code >= 0)
                _dispose = code;
        }

        /**
		 * Sets the number of times the set of GIF frames
		 * should be played.  Default is 1; 0 means play
		 * indefinitely.  Must be invoked before the first
		 * image is added.
		 *
		 * @param iter int number of iterations.
		 * @return
		 */

        public void SetRepeat(int iter)
        {
            if (iter >= 0)
                _repeat = iter;
        }

        /**
		 * Sets the transparent color for the last added frame
		 * and any subsequent frames.
		 * Since all colors are subject to modification
		 * in the quantization process, the color in the final
		 * palette for each frame closest to the given color
		 * becomes the transparent color for that frame.
		 * May be set to null to indicate no transparent color.
		 *
		 * @param c Color to be treated as transparent on display.
		 */

        public void SetTransparent(Color c)
        {
            _transparent = c;
        }

        /**
		 * Adds next GIF frame.  The frame is not written immediately, but is
		 * actually deferred until the next frame is received so that timing
		 * data can be inserted.  Invoking <code>finish()</code> flushes all
		 * frames.  If <code>setSize</code> was not invoked, the size of the
		 * first image is used for all subsequent frames.
		 *
		 * @param im BufferedImage containing frame to write.
		 * @return true if successful.
		 */

        public bool AddFrame(Image im)
        {
            if ((im == null) || !_started)
                return false;
            bool ok = true;
            try {
                if (!_sizeSet)
                    // use first frame's size
                    SetSize(im.Width, im.Height);
                _image = im;
                GetImagePixels(); // convert to correct format if necessary
                AnalyzePixels(); // build color table & map pixels
                if (_firstFrame) {
                    WriteLSD(); // logical screen descriptior
                    WritePalette(); // global color table
                    if (_repeat >= 0)
                        // use NS app extension to indicate reps
                        WriteNetscapeExt();
                }
                WriteGraphicCtrlExt(); // write graphic control extension
                WriteImageDesc(); // image descriptor
                if (!_firstFrame)
                    WritePalette(); // local color table
                WritePixels(); // encode and write pixel data
                _firstFrame = false;
            } catch (IOException) {
                ok = false;
            }

            return ok;
        }

        /**
		 * Flushes any pending data and closes output file.
		 * If writing to an OutputStream, the stream is not
		 * closed.
		 */

        public bool Finish()
        {
            if (!_started) return false;
            bool ok = true;
            _started = false;
            try {
                _fs.WriteByte(0x3b); // gif trailer
                _fs.Flush();
                if (_closeStream)
                    _fs.Close();
            } catch (IOException) {
                ok = false;
            }

            // reset for subsequent use
            _transIndex = 0;
            _fs = null;
            _image = null;
            _pixels = null;
            _indexedPixels = null;
            _colorTab = null;
            _closeStream = false;
            _firstFrame = true;

            return ok;
        }

        /**
		 * Sets quality of color quantization (conversion of images
		 * to the maximum 256 colors allowed by the GIF specification).
		 * Lower values (minimum = 1) produce better colors, but slow
		 * processing significantly.  10 is the default, and produces
		 * good color mapping at reasonable speeds.  Values greater
		 * than 20 do not yield significant improvements in speed.
		 *
		 * @param quality int greater than 0.
		 * @return
		 */

        public void SetQuality(int quality)
        {
            if (quality < 1) quality = 1;
            _sample = quality;
        }

        /**
		 * Sets the GIF frame size.  The default size is the
		 * size of the first frame added if this method is
		 * not invoked.
		 *
		 * @param w int frame width.
		 * @param h int frame width.
		 */

        private void SetSize(int w, int h)
        {
            if (_started && !_firstFrame) return;
            _width = w;
            _height = h;
            if (_width < 1) _width = 320;
            if (_height < 1) _height = 240;
            _sizeSet = true;
        }

        /**
		 * Initiates GIF file creation on the given stream.  The stream
		 * is not closed automatically.
		 *
		 * @param os OutputStream on which GIF images are written.
		 * @return false if initial write failed.
		 */

        private bool Start(FileStream os)
        {
            if (os == null) return false;
            bool ok = true;
            _closeStream = false;
            _fs = os;
            try {
                WriteString("GIF89a"); // header
            } catch (IOException) {
                ok = false;
            }
            return _started = ok;
        }

        /**
		 * Initiates writing of a GIF file with the specified name.
		 *
		 * @param file String containing output file name.
		 * @return false if open or initial write failed.
		 */

        public bool Start(String file)
        {
            bool ok;
            try {
                _fs = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
                ok = Start(_fs);
                _closeStream = true;
            } catch (IOException) {
                ok = false;
            }
            return _started = ok;
        }

        /**
		 * Analyzes image colors and creates color map.
		 */

        private void AnalyzePixels()
        {
            int len = _pixels.Length;
            int nPix = len/3;
            _indexedPixels = new byte[nPix];
            NeuQuant nq = new NeuQuant(_pixels, len, _sample);
            // initialize quantizer
            _colorTab = nq.Process(); // create reduced palette
            // map image pixels to new palette
            int k = 0;
            for (int i = 0; i < nPix; i++) {
                int index = nq.Map(_pixels[k++] & 0xff, _pixels[k++] & 0xff, _pixels[k++] & 0xff);
                _usedEntry[index] = true;
                _indexedPixels[i] = (byte)index;
            }
            _pixels = null;
            _colorDepth = 8;
            _palSize = 7;
            // get closest match to transparent color if specified
            if (_transparent != Color.Empty)
                _transIndex = FindClosest(_transparent);
        }

        /**
		 * Returns index of palette color closest to c
		 *
		 */

        private int FindClosest(Color c)
        {
            if (_colorTab == null) return -1;
            int r = c.R;
            int g = c.G;
            int b = c.B;
            int minpos = 0;
            int dmin = 256*256*256;
            int len = _colorTab.Length;
            for (int i = 0; i < len;) {
                int dr = r - (_colorTab[i++] & 0xff);
                int dg = g - (_colorTab[i++] & 0xff);
                int db = b - (_colorTab[i] & 0xff);
                int d = dr*dr + dg*dg + db*db;
                int index = i/3;
                if (_usedEntry[index] && (d < dmin)) {
                    dmin = d;
                    minpos = index;
                }
                i++;
            }
            return minpos;
        }

        /**
		 * Extracts image pixels into byte array "pixels"
		 */

        private unsafe void GetImagePixels()
        {
            int w = _image.Width;
            int h = _image.Height;
            if ((w != _width) || (h != _height)) {
                Image temp = new Bitmap(_width, _height);
                Graphics g = Graphics.FromImage(temp);
                g.DrawImage(_image, 0, 0);
                _image = temp;
                g.Dispose();
            }
            w = _width;
            h = _height;

            Bitmap tempBitmap = new Bitmap(_image);
            BitmapData bd = tempBitmap.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            byte* p = (byte*)bd.Scan0;
            _pixels = new Byte[3 * w * h];
            int stridePad = bd.Stride - (3*w);
            fixed (byte* v = _pixels) {
                byte* q = v;
                for (int y = 0; y < h; y++, p += stridePad) {
                    for (int x = 0; x < w; x++, q += 3) {
                        *(q+2) = *(p++);
                        *(q+1) = *(p++);
                        *q = *(p++);
                    }
                }
            }
            tempBitmap.UnlockBits(bd);
            tempBitmap.Dispose();
        }

        /**
		 * Writes Graphic Control Extension
		 */

        private void WriteGraphicCtrlExt()
        {
            _fs.WriteByte(0x21); // extension introducer
            _fs.WriteByte(0xf9); // GCE label
            _fs.WriteByte(4); // data block size
            int transp, disp;
            if (_transparent == Color.Empty) {
                transp = 0;
                disp = 0; // dispose = no action
            } else {
                transp = 1;
                disp = 2; // force clear if using transparent color
            }
            if (_dispose >= 0)
                disp = _dispose & 7; // user override
            disp <<= 2;

            // packed fields
            _fs.WriteByte(Convert.ToByte(0 | // 1:3 reserved
                                        disp | // 4:6 disposal
                                        0 | // 7   user input - 0 = none
                                        transp)); // 8   transparency flag

            WriteShort(_delay); // delay x 1/100 sec
            _fs.WriteByte(Convert.ToByte(_transIndex)); // transparent color index
            _fs.WriteByte(0); // block terminator
        }

        /**
		 * Writes Image Descriptor
		 */

        private void WriteImageDesc()
        {
            _fs.WriteByte(0x2c); // image separator
            WriteShort(0); // image position x,y = 0,0
            WriteShort(0);
            WriteShort(_width); // image size
            WriteShort(_height);
            // packed fields
            if (_firstFrame)
                // no LCT  - GCT is used for first (or only) frame
                _fs.WriteByte(0);
            else
                // specify normal LCT
                _fs.WriteByte(Convert.ToByte(0x80 | // 1 local color table  1=yes
                                            0 | // 2 interlace - 0=no
                                            0 | // 3 sorted - 0=no
                                            0 | // 4-5 reserved
                                            _palSize)); // 6-8 size of color table
        }

        /**
		 * Writes Logical Screen Descriptor
		 */

        private void WriteLSD()
        {
            // logical screen size
            WriteShort(_width);
            WriteShort(_height);
            // packed fields
            _fs.WriteByte(Convert.ToByte(0x80 | // 1   : global color table flag = 1 (gct used)
                                        0x70 | // 2-4 : color resolution = 7
                                        0x00 | // 5   : gct sort flag = 0
                                        _palSize)); // 6-8 : gct size

            _fs.WriteByte(0); // background color index
            _fs.WriteByte(0); // pixel aspect ratio - assume 1:1
        }

        /**
		 * Writes Netscape application extension to define
		 * repeat count.
		 */

        private void WriteNetscapeExt()
        {
            _fs.WriteByte(0x21); // extension introducer
            _fs.WriteByte(0xff); // app extension label
            _fs.WriteByte(11); // block size
            WriteString("NETSCAPE" + "2.0"); // app id + auth code
            _fs.WriteByte(3); // sub-block size
            _fs.WriteByte(1); // loop sub-block id
            WriteShort(_repeat); // loop count (extra iterations, 0=repeat forever)
            _fs.WriteByte(0); // block terminator
        }

        /**
		 * Writes color table
		 */

        private void WritePalette()
        {
            _fs.Write(_colorTab, 0, _colorTab.Length);
            int n = (3*256) - _colorTab.Length;
            for (int i = 0; i < n; i++)
                _fs.WriteByte(0);
        }

        /**
		 * Encodes and writes pixel data
		 */

        private void WritePixels()
        {
            LZWEncoder encoder = new LZWEncoder(_indexedPixels, _colorDepth);
            encoder.Encode(_fs);
        }

        /**
		 *    Write 16-bit value to output stream, LSB first
		 */

        private void WriteShort(int value)
        {
            _fs.WriteByte((byte)(value & 0xff));
            _fs.WriteByte((byte)((value >> 8) & 0xff));
        }

        /**
		 * Writes string to output stream
		 */

        private void WriteString(String s)
        {
            byte[] b = Encoding.ASCII.GetBytes(s);
            _fs.Write(b, 0, b.Length);
        }
    }
}