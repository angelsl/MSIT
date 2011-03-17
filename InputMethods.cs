using System;
using System.Collections.Generic;
using MapleLib.WzLib;
using MapleLib.WzLib.WzProperties;

namespace MSreinator
{
    internal class InputMethods
    {
        public static IEnumerable<Frame> InputArgs(IEnumerable<String> args)
        {
            throw new NotImplementedException();
        }

        public static IEnumerable<Frame> InputWz(string wzpath, string inpath, WzMapleVersion wzVer)
        {
            var wz = new WzFile(wzpath, wzVer);
            wz.ParseWzFile();
            var iwah = wz.GetObjectFromPath(wz.WzDirectory.Name + "/" + inpath) as WzSubProperty;
            if (iwah == null) throw new ArgumentException("The path provided did not lead to an animation; check input-wzfile, input-wzpath and input-wzver");
            var r = new List<Frame>();
            foreach (IWzImageProperty iwzo in iwah.WzProperties)
            {
                var iwc = iwzo as WzCanvasProperty;
                if (iwc == null) continue;
                try
                {
                    int n = int.Parse(iwc.Name);
                    r.Add(new Frame(n,
                                    iwc.PngProperty.GetPNG(false),
                                    ((WzVectorProperty) iwc.GetProperty("origin")).Pos,
                                    iwc.GetProperty("delay") != null ? ((WzCompressedIntProperty) iwc.GetProperty("delay")).Value : 100));
                }
                catch
                {
                    continue;
                }
            }
            return r;
        }
    }
}