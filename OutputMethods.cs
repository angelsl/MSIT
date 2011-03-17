// This file is part of MSreinator. This file may have been taken from other applications and libraries.
// 
// MSreinator is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// MSreinator is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MSreinator.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.Linq;
using GifComponents;
using SharpApng;
using X = SharpApng;
using Y = GifComponents;

namespace MSreinator
{
    internal class OutputMethods
    {
        public static void OutputGIF(IEnumerable<Frame> frames, String fn)
        {
            var age = new AnimatedGifEncoder();
            foreach (Frame f in frames)
            {
                age.AddFrame(new GifFrame(f.Image) {Delay = f.Delay/10});
            }
            age.WriteToFile(fn);
        }

        public static void OutputAPNG(IEnumerable<Frame> frames, String fn)
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