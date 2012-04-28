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
using System.Drawing;
using MSIT.WzLib.WzProperties;

namespace MSIT.WzLib
{
    /// <summary>
    ///   An interface for wz objects
    /// </summary>
    public abstract class IWzObject : IDisposable
    {
        /// <summary>
        ///   The name of the object
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        ///   The WzObjectType of the object
        /// </summary>
        public abstract WzObjectType ObjectType { get; }

        /// <summary>
        ///   Returns the parent object
        /// </summary>
        public abstract IWzObject Parent { get; internal set; }

        /// <summary>
        ///   Returns the parent WZ File
        /// </summary>
        public abstract IWzFile WzFileParent { get; }

        public string FullPath
        {
            get
            {
                if (this is WzFile) return ((WzFile) this).WzDirectory.Name;
                string result = Name;
                IWzObject currObj = this;
                while (currObj.Parent != null)
                {
                    currObj = currObj.Parent;
                    result = currObj.Name + @"\" + result;
                }
                return result;
            }
        }

        public virtual object WzValue
        {
            get { return null; }
        }

        #region IDisposable Members

        public abstract void Dispose();

        #endregion

        #region Cast Values

        public static explicit operator float(IWzObject obj)
        {
            return obj.ToFloat(0);
        }

        public static explicit operator int(IWzObject obj)
        {
            return obj.ToInt(0);
        }

        public static explicit operator double(IWzObject obj)
        {
            return obj.ToDouble(0);
        }

        public static explicit operator Bitmap(IWzObject obj)
        {
            return obj.ToBitmap(null);
        }

        public static explicit operator byte[](IWzObject obj)
        {
            return obj.ToBytes(null);
        }

        public static explicit operator string(IWzObject obj)
        {
            return obj.ToString();
        }

        public static explicit operator ushort(IWzObject obj)
        {
            return obj.ToUnsignedShort(0);
        }

        public static explicit operator Point(IWzObject obj)
        {
            return obj.ToPoint(Point.Empty);
        }

        internal virtual float ToFloat(float def)
        {
            return def;
        }

        internal virtual WzPngProperty ToPngProperty(WzPngProperty def)
        {
            /*if (this is WzPngProperty) return (WzPngProperty)this;
            else if (this is WzCanvasProperty) return (WzPngProperty)WzValue;
            else if (this is WzUOLProperty) return ToUOLLink(this).ToPngProperty(def);
            else */
            return def;
        }

        internal virtual int ToInt(int def)
        {
            return def;
        }

        internal virtual double ToDouble(double def)
        {
            return def;
        }

        internal virtual Bitmap ToBitmap(Bitmap def)
        {
            /*if (this is WzPngProperty) return (Bitmap)WzValue;
            else if (this is WzCanvasProperty) return (Bitmap)((WzCanvasProperty)this).PngProperty.WzValue;
            else if (this is WzUOLProperty) return ToUOLLink(this).ToBitmap(def);
            else */
            return def;
        }

        internal virtual byte[] ToBytes(byte[] def)
        {
            /*WzPngProperty png;
            if (this is WzSoundProperty) return (byte[])WzValue;
            else if ((png = (WzPngProperty)this) != null) return png.GetCompressedBytes(false);
            else */
            return def;
        }

        public override string ToString()
        {
            return WzValue.ToString();
        }

        internal virtual ushort ToUnsignedShort(ushort def)
        {
            return def;
        }

        internal virtual Point ToPoint(Point def)
        {
            return def;
        }

        #endregion

        //Credits to BluePoop for the idea of using cast overriding
    }
}