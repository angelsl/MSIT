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

namespace MSIT.WzLib.WzProperties
{
    /// <summary>
    ///   A property that contains a set of properties
    /// </summary>
    public class WzSubProperty : Extended, IPropertyContainer
    {
        #region Fields

        internal string name;
        internal IWzObject parent;
        internal List<IWzImageProperty> properties = new List<IWzImageProperty>();
        //internal WzImage imgParent;

        #endregion

        /// <summary>
        ///   Creates a blank WzSubProperty
        /// </summary>
        public WzSubProperty()
        {
        }

        /// <summary>
        ///   Creates a WzSubProperty with the specified name
        /// </summary>
        /// <param name="name"> The name of the property </param>
        public WzSubProperty(string name)
        {
            this.name = name;
        }

        /// <summary>
        ///   The parent of the object
        /// </summary>
        public override IWzObject Parent
        {
            get { return parent; }
            internal set { parent = value; }
        }

/*		/// <summary>
		/// The image that this property is contained in
		/// </summary>
		public override WzImage ParentImage { get { return imgParent; } internal set { imgParent = value; } }*/

        /// <summary>
        ///   The WzPropertyType of the property
        /// </summary>
        public override WzPropertyType PropertyType
        {
            get { return WzPropertyType.SubProperty; }
        }

        /// <summary>
        ///   The name of the property
        /// </summary>
        public override string Name
        {
            get { return name; }
        }

        #region IPropertyContainer Members

        /// <summary>
        ///   The wz properties contained in the property
        /// </summary>
        public override List<IWzImageProperty> WzProperties
        {
            get { return properties; }
        }

        /// <summary>
        ///   Gets a wz property by it's name
        /// </summary>
        /// <param name="pName"> The name of the property </param>
        /// <returns> The wz property with the specified name </returns>
        public override IWzImageProperty this[string pName]
        {
            get
            {
                foreach (IWzImageProperty iwp in properties) if (iwp.Name.ToLower() == pName.ToLower()) return iwp;
                //throw new KeyNotFoundException("A wz property with the specified name was not found");
                return null;
            }
        }

        /// <summary>
        ///   Adds a property to the list
        /// </summary>
        /// <param name="prop"> The property to add </param>
        public void AddProperty(IWzImageProperty prop)
        {
            prop.Parent = this;
            properties.Add(prop);
        }

        public void AddProperties(List<IWzImageProperty> props)
        {
            foreach (IWzImageProperty prop in props)
            {
                AddProperty(prop);
            }
        }

        public void RemoveProperty(IWzImageProperty prop)
        {
            prop.Parent = null;
            properties.Remove(prop);
        }

        /// <summary>
        ///   Clears the list of properties
        /// </summary>
        public void ClearProperties()
        {
            foreach (IWzImageProperty prop in properties) prop.Parent = null;
            properties.Clear();
        }

        #endregion

        public override IWzImageProperty DeepClone()
        {
            WzSubProperty clone = (WzSubProperty) MemberwiseClone();
            clone.properties = new List<IWzImageProperty>();
            foreach (IWzImageProperty prop in properties) clone.properties.Add(prop.DeepClone());
            return clone;
        }

        /// <summary>
        ///   Gets a wz property by a path name
        /// </summary>
        /// <param name="path"> path to property </param>
        /// <returns> the wz property with the specified name </returns>
        public override IWzImageProperty GetFromPath(string path)
        {
            string[] segments = path.Split(new char[1] {'/'}, StringSplitOptions.RemoveEmptyEntries);
            if (segments[0] == "..")
            {
                return ((IWzImageProperty) Parent)[path.Substring(name.IndexOf('/') + 1)];
            }
            IWzImageProperty ret = this;
            for (int x = 0; x < segments.Length; x++)
            {
                bool foundChild = false;
                foreach (IWzImageProperty iwp in ret.WzProperties)
                {
                    if (iwp.Name == segments[x])
                    {
                        ret = iwp;
                        foundChild = true;
                        break;
                    }
                }
                if (!foundChild)
                {
                    return null;
                }
            }
            return ret;
        }

        /// <summary>
        ///   Disposes the object
        /// </summary>
        public override void Dispose()
        {
            name = null;
            foreach (IWzImageProperty prop in properties) prop.Dispose();
            properties.Clear();
            properties = null;
        }
    }
}