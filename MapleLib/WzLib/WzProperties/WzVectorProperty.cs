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
using System.Drawing;
using System.IO;
using MapleLib.WzLib.Util;

namespace MapleLib.WzLib.WzProperties
{
    /// <summary>
    /// A property that contains an x and a y value
    /// </summary>
    public class WzVectorProperty : IExtended
    {
        #region Fields

        internal string name;
        internal IWzObject parent;
        internal WzCompressedIntProperty x, y;
        //internal WzImage imgParent;

        #endregion

        /// <summary>
        /// Creates a blank WzVectorProperty
        /// </summary>
        public WzVectorProperty()
        {
        }

        /// <summary>
        /// Creates a WzVectorProperty with the specified name
        /// </summary>
        /// <param name="name">The name of the property</param>
        public WzVectorProperty(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// Creates a WzVectorProperty with the specified name, x and y
        /// </summary>
        /// <param name="name">The name of the property</param>
        /// <param name="x">The x value of the vector</param>
        /// <param name="y">The y value of the vector</param>
        public WzVectorProperty(string name, WzCompressedIntProperty x, WzCompressedIntProperty y)
        {
            this.name = name;
            this.x = x;
            this.y = y;
        }

        #region Cast Values

        internal override Point ToPoint(Point def)
        {
            return new Point(x.val, y.val);
        }

        public override string ToString()
        {
            return "X: " + x.val + ", Y: " + y.val;
        }

        #endregion

        public override object WzValue
        {
            get { return new Point(x.Value, y.Value); }
        }

        /// <summary>
        /// The parent of the object
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
        /// The name of the property
        /// </summary>
        public override string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// The WzPropertyType of the property
        /// </summary>
        public override WzPropertyType PropertyType
        {
            get { return WzPropertyType.Vector; }
        }

        /// <summary>
        /// The X value of the Vector2D
        /// </summary>
        public WzCompressedIntProperty X
        {
            get { return x; }
            set { x = value; }
        }

        /// <summary>
        /// The Y value of the Vector2D
        /// </summary>
        public WzCompressedIntProperty Y
        {
            get { return y; }
            set { y = value; }
        }

        /// <summary>
        /// The Point of the Vector2D created from the X and Y
        /// </summary>
        public Point Pos
        {
            get { return new Point(X.Value, Y.Value); }
        }

        public override void SetValue(object value)
        {
            if (value is Point)
            {
                x.val = ((Point) value).X;
                y.val = ((Point) value).Y;
            }
            else
            {
                x.val = ((Size) value).Width;
                y.val = ((Size) value).Height;
            }
        }

        public override IWzImageProperty DeepClone()
        {
            var clone = (WzVectorProperty) MemberwiseClone();
            clone.x = (WzCompressedIntProperty) x.DeepClone();
            clone.y = (WzCompressedIntProperty) y.DeepClone();
            return clone;
        }

        public override void WriteValue(WzBinaryWriter writer)
        {
            writer.WriteStringValue("Shape2D#Vector2D", 0x73, 0x1B);
            writer.WriteCompressedInt(X.Value);
            writer.WriteCompressedInt(Y.Value);
        }

        public override void ExportXml(StreamWriter writer, int level)
        {
            writer.WriteLine(XmlUtil.Indentation(level) + XmlUtil.OpenNamedTag("WzVector", Name, false, false) + XmlUtil.Attrib("X", X.Value.ToString()) +
                             XmlUtil.Attrib("Y", Y.Value.ToString(), true, true));
        }

        /// <summary>
        /// Disposes the object
        /// </summary>
        public override void Dispose()
        {
            name = null;
            x.Dispose();
            x = null;
            y.Dispose();
            y = null;
        }
    }
}