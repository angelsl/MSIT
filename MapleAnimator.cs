using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace MSreinator
{
    internal class OffsetAnimator
    {
        // Algorithm stolen from haha01haha01 http://code.google.com/p/hasuite/source/browse/trunk/HaRepackerLib/AnimationBuilder.cs
        public static IEnumerable<Frame> Process(IEnumerable<Frame> frames, Rectangle padding, Color background)
        {
            Size lis = GetLargestImageSize(frames);
            Size smp = GetSmallestPadding(frames, lis);
            Point lbr = GetLargestBottomRight(frames, lis);
            Size fs = GetFrameSize(lbr, smp, padding);
            frames = NormaliseOffsets(frames, lis, smp);
            return FinalProcess(frames, padding, background, fs);
        }

        private static Size GetLargestImageSize(IEnumerable<Frame> frames)
        {
            return new Size(frames.Max(f => f.Image.Width), frames.Max(f => f.Image.Height));
        }

        private static Size GetSmallestPadding(IEnumerable<Frame> frames, Size limits)
        {
            return new Size(frames.Min(f => limits.Width - f.Offset.X), frames.Min(f => limits.Height - f.Offset.Y));
        }

        private static Point GetLargestBottomRight(IEnumerable<Frame> frames, Size limits)
        {
            return new Point(frames.Max(f => (limits.Width - f.Offset.X) + f.Image.Width), frames.Max(f => (limits.Height - f.Offset.Y) + f.Image.Height));
        }

        private static Size GetFrameSize(Point largestBottomRight, Size smallestPadding, Rectangle padding)
        {
            return new Size((largestBottomRight.X - smallestPadding.Width) +padding.X + padding.Width, (largestBottomRight.Y - smallestPadding.Height) + padding.Y+padding.Height);
        }


        private static IEnumerable<Frame> NormaliseOffsets(IEnumerable<Frame> frames, Size lis, Size smp)
        {
            return frames.Select(f => new Frame(f.Number, f.Image, new Point(lis.Width - smp.Width - f.Offset.X, lis.Height - smp.Height - f.Offset.Y), f.Delay));
        }

        private static IEnumerable<Frame> FinalProcess(IEnumerable<Frame> frames, Rectangle padding, Color background, Size fsize)
        {
            return frames.Select(f =>
                                     {
                                         var b = new Bitmap(fsize.Width, fsize.Height);
                                         Graphics g = Graphics.FromImage(b);
                                         g.FillRectangle(new SolidBrush(background), 0, 0, fsize.Width, fsize.Height);
                                         g.DrawImage(f.Image, new Point(f.Offset.X + padding.X, f.Offset.Y + padding.Y));
                                         g.Flush(FlushIntention.Flush);
                                         return new Frame(f.Number, b, new Point(0, 0), f.Delay);
                                     });
        }
    }
}