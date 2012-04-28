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

namespace MSIT.WzLib.WzProperties
{
    /// <summary>
    ///   A wz property which has a value which is a ushort
    /// </summary>
    public class WzUnsignedShortProperty : IWzImageProperty
    {
        #region Fields

        internal string name;
        internal IWzObject parent;
        internal ushort val;
        //internal WzImage imgParent;

        #endregion

        /// <summary>
        ///   Creates a blank WzUnsignedShortProperty
        /// </summary>
        public WzUnsignedShortProperty()
        {
        }

        /// <summary>
        ///   Creates a WzUnsignedShortProperty with the specified name
        /// </summary>
        /// <param name="name"> The name of the property </param>
        public WzUnsignedShortProperty(string name)
        {
            this.name = name;
        }

        /// <summary>
        ///   Creates a WzUnsignedShortProperty with the specified name and value
        /// </summary>
        /// <param name="name"> The name of the property </param>
        /// <param name="value"> The value of the property </param>
        public WzUnsignedShortProperty(string name, ushort value)
        {
            this.name = name;
            val = value;
        }

        #region Cast Values

        internal override float ToFloat(float def)
        {
            return val;
        }

        internal override double ToDouble(double def)
        {
            return val;
        }

        internal override int ToInt(int def)
        {
            return val;
        }

        internal override ushort ToUnsignedShort(ushort def)
        {
            return val;
        }

        #endregion

        public override object WzValue
        {
            get { return Value; }
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
        ///   The WzPropertyType of the property
        /// </summary>
        public override WzPropertyType PropertyType
        {
            get { return WzPropertyType.UnsignedShort; }
        }

        /// <summary>
        ///   The name of the property
        /// </summary>
        public override string Name
        {
            get { return name; }
        }

        /// <summary>
        ///   The value of the property
        /// </summary>
        public ushort Value
        {
            get { return val; }
        }

        public override IWzImageProperty DeepClone()
        {
            WzUnsignedShortProperty clone = (WzUnsignedShortProperty) MemberwiseClone();
            return clone;
        }

        /// <summary>
        ///   Disposes the object
        /// </summary>
        public override void Dispose()
        {
            name = null;
        }
    }
}