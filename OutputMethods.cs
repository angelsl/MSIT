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
            age.Dispose();
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