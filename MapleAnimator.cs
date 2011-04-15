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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace MSIT
{
    internal class OffsetAnimator
    {
        // Algorithm stolen from haha01haha01 http://code.google.com/p/hasuite/source/browse/trunk/HaRepackerLib/AnimationBuilder.cs
        public static IEnumerable<Frame> Process(Rectangle padding, Color background, params IEnumerable<Frame>[] zframess)
        {
            var framess = zframess.Select(aframess => aframess.Select(f => new Frame(f.Number, f.Image, new Point(-f.Offset.X, -f.Offset.Y), f.Delay)));
            framess = PadOffsets(NormaliseOffsets(framess), padding);
            Size fs = GetFrameSize(framess, padding);
            var frames = MergeMultiple(framess, fs, background);
            return FinalProcess(frames, fs, background);
        }

        private static IEnumerable<IEnumerable<Frame>> NormaliseOffsets(IEnumerable<IEnumerable<Frame>> framess)
        {
            int minx = framess.SelectMany(x => x).Min(fy => fy.Offset.X);
            int miny = framess.SelectMany(x => x).Min(fy => fy.Offset.Y);

            return framess.Select(fx => fx.Select(fy => new Frame(fy.Number, fy.Image, new Point(fy.Offset.X - minx, fy.Offset.Y - miny), fy.Delay)));
        }

        private static IEnumerable<IEnumerable<Frame>> PadOffsets(IEnumerable<IEnumerable<Frame>> framess, Rectangle p)
        {
            return framess.Select(fx => fx.Select(fy => new Frame(fy.Number, fy.Image, new Point(fy.Offset.X + p.X, fy.Offset.Y + p.Y), fy.Delay)));
        }

        private static Size GetFrameSize(IEnumerable<IEnumerable<Frame>> framess, Rectangle padding)
        {
            int w = framess.SelectMany(x => x).Min(x => padding.X + x.Offset.X + x.Image.Width + padding.Width);
            int h = framess.SelectMany(x => x).Min(x => padding.Y + x.Offset.Y + x.Image.Height + padding.Height);
            return new Size(w, h);
            
        }

        private static IEnumerable<Frame> MergeMultiple(IEnumerable<IEnumerable<Frame>> framess, Size fs, Color bg)
        {
            if (framess.Count() == 1) return framess.First();
            List<Frame> merged = new List<Frame>();
            List<IEnumerator<Frame>> ers = framess.Select(x => x.GetEnumerator()).ToList();
            foreach(var e in ers)
            {
                e.Reset();
                e.MoveNext();
            }
            int no = 0;
            while(ers.Count > 0)
            {
                int mindelay = ers.Min(x => x.Current.Delay);
                foreach(var e in ers)
                {
                    e.Current.Delay -= mindelay;
                }
                Bitmap b = new Bitmap(fs.Width, fs.Height);
                Graphics g = Graphics.FromImage(b);
                g.FillRectangle(new SolidBrush(bg), 0, 0, b.Width, b.Height);
                foreach(var e in ers)
                {
                    g.DrawImage(e.Current.Image, e.Current.Offset);
                }
                g.Flush(FlushIntention.Sync);
                merged.Add(new Frame(no++, b, new Point(0,0), mindelay));
                var todel = ers.Where(e => e.Current.Delay <= 0).Where(e => !e.MoveNext());
                ers.RemoveAll(c => todel.Contains(c));
            }
            return merged;
        }

        private static IEnumerable<Frame> FinalProcess(IEnumerable<Frame> frame, Size fs, Color bg)
        {
            return frame.Select(n =>
                                    {
                                        Bitmap b = new Bitmap(fs.Width, fs.Height);
                                        Graphics g = Graphics.FromImage(b);
                                        g.FillRectangle(new SolidBrush(bg), 0, 0, b.Width, b.Height);
                                        g.DrawImage(n.Image, n.Offset);
                                        g.Flush(FlushIntention.Sync);
                                        return new Frame(n.Number, b, new Point(0, 0), n.Delay);
                                    });
        }
    }
}