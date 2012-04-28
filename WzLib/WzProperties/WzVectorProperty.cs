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

using System.Drawing;

namespace MSIT.WzLib.WzProperties
{
    /// <summary>
    ///   A property that contains an x and a y value
    /// </summary>
    public class WzVectorProperty : Extended
    {
        #region Fields

        internal string name;
        internal IWzObject parent;
        internal WzCompressedIntProperty x, y;
        //internal WzImage imgParent;

        #endregion

        /// <summary>
        ///   Creates a blank WzVectorProperty
        /// </summary>
        public WzVectorProperty()
        {
        }

        /// <summary>
        ///   Creates a WzVectorProperty with the specified name
        /// </summary>
        /// <param name="name"> The name of the property </param>
        public WzVectorProperty(string name)
        {
            this.name = name;
        }

        /// <summary>
        ///   Creates a WzVectorProperty with the specified name, x and y
        /// </summary>
        /// <param name="name"> The name of the property </param>
        /// <param name="x"> The x value of the vector </param>
        /// <param name="y"> The y value of the vector </param>
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

        /// <summary>
        ///   The WzPropertyType of the property
        /// </summary>
        public override WzPropertyType PropertyType
        {
            get { return WzPropertyType.Vector; }
        }

        /// <summary>
        ///   The X value of the Vector2D
        /// </summary>
        public WzCompressedIntProperty X
        {
            get { return x; }
            set { x = value; }
        }

        /// <summary>
        ///   The Y value of the Vector2D
        /// </summary>
        public WzCompressedIntProperty Y
        {
            get { return y; }
            set { y = value; }
        }

        /// <summary>
        ///   The Point of the Vector2D created from the X and Y
        /// </summary>
        public Point Pos
        {
            get { return new Point(X.Value, Y.Value); }
        }

        public override IWzImageProperty DeepClone()
        {
            WzVectorProperty clone = (WzVectorProperty) MemberwiseClone();
            clone.x = (WzCompressedIntProperty) x.DeepClone();
            clone.y = (WzCompressedIntProperty) y.DeepClone();
            return clone;
        }

        /// <summary>
        ///   Disposes the object
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