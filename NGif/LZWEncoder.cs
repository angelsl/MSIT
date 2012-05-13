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

#region Java

//==============================================================================
//  Adapted from Jef Poskanzer's Java port by way of J. M. G. Elliott.
//  K Weiner 12/00

#endregion

using System;
using System.IO;

namespace MSIT.NGif
{
    public class LZWEncoder
    {
        private const int EOF = -1;

        // GIFCOMPR.C       - GIF Image compression routines
        //
        // Lempel-Ziv compression based on 'compress'.  GIF modifications by
        // David Rowley (mgardi@watdcsu.waterloo.edu)

        // General DEFINEs

        private const int Bits = 12;

        private const int HSize = 5003; // 80% occupancy
        private readonly byte[] _accum = new byte[256];

        // GIF Image compression - modified 'compress'
        //
        // Based on: compress.c - File compression ala IEEE Computer, June 1984.
        //
        // By Authors:  Spencer W. Thomas      (decvax!harpo!utah-cs!utah-gr!thomas)
        //              Jim McKie              (decvax!mcvax!jim)
        //              Steve Davies           (decvax!vax135!petsd!peora!srd)
        //              Ken Turkowski          (decvax!decwrl!turtlevax!ken)
        //              James A. Woods         (decvax!ihnp4!ames!jaw)
        //              Joe Orost              (decvax!vax135!petsd!joe)

        private readonly int[] _codetab = new int[HSize];

        private readonly int[] _htab = new int[HSize];
        private readonly int _initCodeSize;
        private readonly int[] _masks = {0x0000, 0x0001, 0x0003, 0x0007, 0x000F, 0x001F, 0x003F, 0x007F, 0x00FF, 0x01FF, 0x03FF, 0x07FF, 0x0FFF, 0x1FFF, 0x3FFF, 0x7FFF, 0xFFFF};
        private const int MaxMaxCode = 1 << Bits; // should NEVER generate this code
        private readonly byte[] _pixAry;

        private int _clearCode;
        private int _eofCode;
        private int _aCount;
        private bool _clearFlg;
        private int _curPixel;

        // output
        //
        // Output the given code.
        // Inputs:
        //      code:   A n_bits-bit integer.  If == -1, then EOF.  This assumes
        //              that n_bits =< wordsize - 1.
        // Outputs:
        //      Outputs code to the file.
        // Assumptions:
        //      Chars are 8 bits long.
        // Algorithm:
        //      Maintain a BITS character long buffer (so that 8 codes will
        // fit in it exactly).  Use the VAX insv instruction to insert each
        // code in turn.  When the buffer fills up empty it and start over.

        private int _curAccum;
        private int _curBits;
        private int _freeEnt; // first unused entry
        private int _gInitBits;
        private int _maxcode; // maximum code, given n_bits
        private int _nBits; // number of bits/code

        //----------------------------------------------------------------------------
        public LZWEncoder(byte[] pixels, int colorDepth)
        {
            _pixAry = pixels;
            _initCodeSize = Math.Max(2, colorDepth);
        }

        // Add a character to the end of the current packet, and if it is 254
        // characters, flush the packet to disk.
        private void Add(byte c, Stream outs)
        {
            _accum[_aCount++] = c;
            if (_aCount >= 254) Flush(outs);
        }

        // Clear out the hash table

        // table clear for block compress
        private void ClearTable(Stream outs)
        {
            ResetCodeTable(HSize);
            _freeEnt = _clearCode + 2;
            _clearFlg = true;

            Output(_clearCode, outs);
        }

        // reset code table
        private void ResetCodeTable(int hsizep)
        {
            for (int i = 0; i < hsizep; ++i) _htab[i] = -1;
        }

        private void Compress(int initBits, Stream outs)
        {
            // Set up the globals:  g_init_bits - initial number of bits
            _gInitBits = initBits;

            // Set up the necessary values
            _clearFlg = false;
            _nBits = _gInitBits;
            _maxcode = MaxCode(_nBits);

            _clearCode = 1 << (initBits - 1);
            _eofCode = _clearCode + 1;
            _freeEnt = _clearCode + 2;

            _aCount = 0; // clear packet

            int ent = NextPixel();

            int hshift = 0;
            for (int i = HSize; i < 65536; i *= 2) ++hshift;
            hshift = 8 - hshift; // set hash code range bound
            ResetCodeTable(HSize); // clear hash table

            Output(_clearCode, outs);

            int c;
            outer_loop:
            while ((c = NextPixel()) != EOF) {
                int fcode = (c << Bits) + ent;
                int i = (c << hshift) ^ ent /* = 0 */;

                if (_htab[i] == fcode) {
                    ent = _codetab[i];
                    continue;
                }
                if (_htab[i] >= 0) // non-empty slot
                {
                    int disp = HSize - i;
                    if (i == 0) disp = 1;
                    do {
                        if ((i -= disp) < 0) i += HSize;

                        if (_htab[i] != fcode) continue;
                        ent = _codetab[i];
                        goto outer_loop;
                    } while (_htab[i] >= 0);
                }
                Output(ent, outs);
                ent = c;
                if (_freeEnt < MaxMaxCode) {
                    _codetab[i] = _freeEnt++; // code -> hashtable
                    _htab[i] = fcode;
                } else ClearTable(outs);
            }
            // Put out the final code.
            Output(ent, outs);
            Output(_eofCode, outs);
        }

        //----------------------------------------------------------------------------
        public void Encode(Stream os)
        {
            os.WriteByte((byte)_initCodeSize); // write "initial code size" byte
            _curPixel = 0;
            Compress(_initCodeSize + 1, os); // compress and write the pixel data
            os.WriteByte(0); // write block terminator
        }

        // Flush the packet to disk, and reset the accumulator
        private void Flush(Stream outs)
        {
            if (_aCount <= 0) return;
            outs.WriteByte(Convert.ToByte(_aCount));
            outs.Write(_accum, 0, _aCount);
            _aCount = 0;
        }

        private static int MaxCode(int nBitsp)
        {
            return (1 << nBitsp) - 1;
        }

        //----------------------------------------------------------------------------
        // Return the next pixel from the image
        //----------------------------------------------------------------------------

        private int NextPixel()
        {
            if (_curPixel <= _pixAry.GetUpperBound(0)) {
                byte pix = _pixAry[_curPixel++];
                return pix & 0xff;
            }
            return EOF;
        }

        private void Output(int code, Stream outs)
        {
            _curAccum &= _masks[_curBits];

            if (_curBits > 0) _curAccum |= (code << _curBits);
            else _curAccum = code;

            _curBits += _nBits;

            while (_curBits >= 8) {
                Add((byte)(_curAccum & 0xff), outs);
                _curAccum >>= 8;
                _curBits -= 8;
            }

            // If the next entry is going to be too big for the code size,
            // then increase it, if possible.
            if (_freeEnt > _maxcode || _clearFlg)
                if (_clearFlg) {
                    _maxcode = MaxCode(_nBits = _gInitBits);
                    _clearFlg = false;
                } else {
                    ++_nBits;
                    _maxcode = _nBits == Bits ? MaxMaxCode : MaxCode(_nBits);
                }

            if (code != _eofCode) return;
            // At EOF, write the rest of the buffer.
            while (_curBits > 0) {
                Add((byte)(_curAccum & 0xff), outs);
                _curAccum >>= 8;
                _curBits -= 8;
            }

            Flush(outs);
        }
    }
}