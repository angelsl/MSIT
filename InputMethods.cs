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
using MapleLib.WzLib;
using MapleLib.WzLib.WzProperties;

namespace MSIT
{
    internal class InputMethods
    {


        public static List<Frame> InputWz(WzFile wz, string inpath)
        {
            
            var iwah = wz.GetWzObjectFromPath(inpath) as WzSubProperty;
            if (iwah == null) throw new ArgumentException("The path provided did not lead to an animation; check input-wzfile, input-wzpath and input-wzver");
            var r = new List<Frame>();
            foreach (IWzImageProperty iwzo in iwah.WzProperties)
            {
                var iwc = (iwzo is WzUOLProperty ? ((WzUOLProperty) iwzo).Resolve() : iwzo) as WzCanvasProperty;
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

    internal static class WzUtilities
    {
        public static IWzObject GetWzObjectFromPath(this WzFile wz, string path)
        {
            return wz.GetObjectFromPath(wz.WzDirectory.Name + "/" + path);
        }

        public static IWzObject Resolve(this WzUOLProperty uol)
        {
            IWzObject ret = uol.LinkValue;
            while (ret is WzUOLProperty)
                ret = ((WzUOLProperty) ret).LinkValue;
            return ret;
        }
    }
}