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

#define UOLRES

using System;
using System.Collections.Generic;
using System.Drawing;

namespace MSIT.WzLib.WzProperties
{
    /// <summary>
    ///   A property that's value is a string
    /// </summary>
    public class WzUOLProperty : Extended
    {
        #region Fields

        internal string name, val;
        internal IWzObject parent;
        //internal WzImage imgParent;
        internal IWzObject linkVal;

        #endregion

        #region Inherited Members

        public override IWzImageProperty DeepClone()
        {
            WzUOLProperty clone = (WzUOLProperty) MemberwiseClone();
            clone.linkVal = null;
            return clone;
        }

        public override object WzValue
        {
            get
            {
#if UOLRES
                return LinkValue;
#else
                return this;
#endif
            }
        }

        /// <summary>
        ///   The parent of the object
        /// </summary>
        public override IWzObject Parent
        {
            get { return parent; }
            internal set { parent = value; }
        }

        /*/// <summary>
		/// The image that this property is contained in
		/// </summary>
		public override WzImage ParentImage { get { return imgParent; } internal set { imgParent = value; } }*/

        /// <summary>
        ///   The name of the property
        /// </summary>
        public override string Name
        {
            get { return name; }
        }

#if UOLRES
        public override List<IWzImageProperty> WzProperties
        {
            get { return LinkValue is IWzImageProperty ? ((IWzImageProperty) LinkValue).WzProperties : null; }
        }


        public override IWzImageProperty this[string name]
        {
            get { return LinkValue is IWzImageProperty ? ((IWzImageProperty) LinkValue)[name] : LinkValue is WzImage ? ((WzImage) LinkValue)[name] : null; }
        }

        public override IWzImageProperty GetFromPath(string path)
        {
            return LinkValue is IWzImageProperty ? ((IWzImageProperty) LinkValue).GetFromPath(path) : LinkValue is WzImage ? ((WzImage) LinkValue).GetFromPath(path) : null;
        }
#endif

        /// <summary>
        ///   The WzPropertyType of the property
        /// </summary>
        public override WzPropertyType PropertyType
        {
            get { return WzPropertyType.UOL; }
        }

        /// <summary>
        ///   Disposes the object
        /// </summary>
        public override void Dispose()
        {
            name = null;
            val = null;
        }

        #endregion

        #region Custom Members

        /// <summary>
        ///   The value of the property
        /// </summary>
        public string Value
        {
            get { return val; }
            set { val = value; }
        }

#if UOLRES
        public IWzObject LinkValue
        {
            get
            {
                if (linkVal == null)
                {
                    string[] paths = val.Split('/');
                    linkVal = parent;
                    string asdf = parent.FullPath;
                    foreach (string path in paths)
                    {
                        if (path == "..")
                        {
                            linkVal = linkVal.Parent;
                        }
                        else
                        {
                            if (linkVal is IWzImageProperty) linkVal = ((IWzImageProperty) linkVal)[path];
                            else if (linkVal is WzImage) linkVal = ((WzImage) linkVal)[path];
                            else if (linkVal is WzDirectory) linkVal = ((WzDirectory) linkVal)[path];
                            else throw new Exception("Invalid linkVal");
                        }
                    }
                }
                return linkVal;
            }
        }
#endif

        /// <summary>
        ///   Creates a blank WzUOLProperty
        /// </summary>
        public WzUOLProperty()
        {
        }

        /// <summary>
        ///   Creates a WzUOLProperty with the specified name
        /// </summary>
        /// <param name="name"> The name of the property </param>
        public WzUOLProperty(string name)
        {
            this.name = name;
        }

        /// <summary>
        ///   Creates a WzUOLProperty with the specified name and value
        /// </summary>
        /// <param name="name"> The name of the property </param>
        /// <param name="value"> The value of the property </param>
        public WzUOLProperty(string name, string value)
        {
            this.name = name;
            val = value;
        }

        #endregion

        #region Cast Values

#if UOLRES
        internal override Bitmap ToBitmap(Bitmap def)
        {
            return LinkValue.ToBitmap(def);
        }

        internal override byte[] ToBytes(byte[] def)
        {
            return LinkValue.ToBytes(def);
        }

        internal override double ToDouble(double def)
        {
            return LinkValue.ToDouble(def);
        }

        internal override float ToFloat(float def)
        {
            return LinkValue.ToFloat(def);
        }

        internal override int ToInt(int def)
        {
            return LinkValue.ToInt(def);
        }

        internal override WzPngProperty ToPngProperty(WzPngProperty def)
        {
            return LinkValue.ToPngProperty(def);
        }

        internal override Point ToPoint(Point def)
        {
            return LinkValue.ToPoint(def);
        }

        public override string ToString()
        {
            return val;
            //return LinkValue.ToString();
        }

        internal override ushort ToUnsignedShort(ushort def)
        {
            return LinkValue.ToUnsignedShort(def);
        }

#else
        public override string ToString()
        {
            return val;
        }
#endif

        #endregion
    }
}