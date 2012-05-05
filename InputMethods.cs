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
using reWZ;
using reWZ.WZProperties;

namespace MSIT
{
    internal static class InputMethods
    {
        public static List<Frame> InputWz(WZFile wz, string inpath)
        {
            WZObject iwahz = wz.ResolvePath(inpath);
            WZSubProperty iwah = iwahz as WZSubProperty;
            if (iwah == null) throw new ArgumentException("The path provided did not lead to an animation; check input-wzfile, input-wzpath and input-wzver");
            List<Frame> r = new List<Frame>();
            foreach (WZObject iwzo in iwah) {
                WZCanvasProperty iwc = (iwzo is WZUOLProperty ? ((WZUOLProperty)iwzo).ResolveFully() : iwzo) as WZCanvasProperty;
                if (iwc == null) continue;
                int n;
                if (!int.TryParse(iwzo.Name, out n)) continue;
                r.Add(new Frame(n, iwc.Value, ((WZPointProperty)iwc["origin"]).Value, iwc.ContainsKey("delay") ? iwc["delay"].ToInt() : 100));
            }
            return r.OrderBy(f => f.Number).ToList();
        }
    }

    internal static class WzUtilities
    {

        public static int ToInt(this WZObject izo)
        {
            WZInt32Property wzInt32Property = izo as WZInt32Property;
            if (wzInt32Property != null) return (wzInt32Property).Value;
            WZStringProperty wzStringProperty = izo as WZStringProperty;
            if (wzStringProperty != null) return int.Parse((wzStringProperty).Value);
            throw new FormatException(String.Format("Cannot convert {0} to integer; is not an integer.", izo.GetType().Name));
        }
    }
}