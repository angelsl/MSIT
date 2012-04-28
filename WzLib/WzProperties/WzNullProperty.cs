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
    ///   A property that's value is null
    /// </summary>
    public class WzNullProperty : IWzImageProperty
    {
        #region Fields

        internal string name;
        internal IWzObject parent;
        //internal WzImage imgParent;

        #endregion

        /// <summary>
        ///   Creates a blank WzNullProperty
        /// </summary>
        public WzNullProperty()
        {
        }

        /// <summary>
        ///   Creates a WzNullProperty with the specified name
        /// </summary>
        /// <param name="propName"> The name of the property </param>
        public WzNullProperty(string propName)
        {
            name = propName;
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
            get { return WzPropertyType.Null; }
        }

        /// <summary>
        ///   The name of the property
        /// </summary>
        public override string Name
        {
            get { return name; }
        }

        /// <summary>
        ///   The WzObjectType of the property
        /// </summary>
        public override WzObjectType ObjectType
        {
            get { return WzObjectType.Property; }
        }

        public override IWzImageProperty DeepClone()
        {
            WzNullProperty clone = (WzNullProperty) MemberwiseClone();
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