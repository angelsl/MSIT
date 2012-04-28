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
using System.Collections.Generic;
using System.Linq;
using MSIT.WzLib;
using MSIT.WzLib.WzProperties;

namespace MSIT
{
    internal class InputMethods
    {
        public static List<Frame> InputWz(WzFile wz, string inpath)
        {
            IWzObject iwahz = wz.GetWzObjectFromPath(inpath);
            WzSubProperty iwah = iwahz as WzSubProperty;
            if (iwah == null) throw new ArgumentException("The path provided did not lead to an animation; check input-wzfile, input-wzpath and input-wzver");
            List<Frame> r = new List<Frame>();
            foreach (IWzImageProperty iwzo in iwah.WzProperties)
            {
                WzCanvasProperty iwc = (iwzo is WzUOLProperty ? ((WzUOLProperty) iwzo).Resolve() : iwzo) as WzCanvasProperty;
                if (iwc == null) continue;
                int n;
                if (!int.TryParse(iwzo.Name, out n)) continue;
                r.Add(new Frame(n, iwc.PngProperty.GetPng(false), ((WzVectorProperty) iwc.GetProperty("origin")).Pos, iwc.GetProperty("delay") != null ? iwc.GetProperty("delay").ToInt() : 100));
            }
            return r.OrderBy(f => f.Number).ToList();
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
            while (ret is WzUOLProperty) ret = ((WzUOLProperty) ret).LinkValue;
            return ret;
        }

        public static int ToInt(this IWzObject izo)
        {
            if (izo is WzCompressedIntProperty)
            {
                return ((WzCompressedIntProperty) izo).Value;
            }
            else if (izo is WzStringProperty)
            {
                return int.Parse(((WzStringProperty) izo).Value);
            }
            else throw new InvalidOperationException(String.Format("Cannot convert {0} to integer.", izo.GetType().Name));
        }
    }
}