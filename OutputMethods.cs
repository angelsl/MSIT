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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using SharpApng;
using X = SharpApng;
using Y = NGif;

namespace MSIT
{
    internal class OutputMethods
    {
        public static void OutputGIF(IEnumerable<Frame> frames, String fn)
        {
            Y.AnimatedGifEncoder gif = new Y.AnimatedGifEncoder();
            gif.SetQuality(4);
            gif.SetRepeat(0);
            gif.Start(fn);
            foreach(Frame f in frames)
            {
                gif.SetDelay(f.Delay);
                gif.AddFrame(f.Image);
            }
            gif.Finish();
        }

        public static void OutputPNG(IEnumerable<Frame> frames, String fn)
        {
            frames = frames.OrderBy(f => f.Number);
            var apng = new Apng();
            foreach (Frame f in frames)
            {
                apng.AddFrame(f.Image, f.Delay, 1000);
            }
            apng.WriteApng(fn, false, true);
        }
    }
}