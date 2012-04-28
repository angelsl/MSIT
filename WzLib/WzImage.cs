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
using System.IO;
using MSIT.WzLib.Util;
using MSIT.WzLib.WzProperties;

namespace MSIT.WzLib
{
    /// <summary>
    ///   A .img contained in a wz directory
    /// </summary>
    public class WzImage : IWzObject, IPropertyContainer
    {
        #region Fields

        internal int blockStart;
        internal bool changed;
        internal int checksum;
        internal bool isEncrypted = true;
        internal string name;
        internal uint offset;
        internal IWzObject parent;
        internal bool parseEverything;
        internal bool parsed;
        internal List<IWzImageProperty> properties = new List<IWzImageProperty>();
        internal WzBinaryReader reader;
        internal int size;
        internal long tempFileEnd;
        internal long tempFileStart;

        #endregion

        #region Constructors\Destructors

        /// <summary>
        ///   Creates a blank WzImage
        /// </summary>
        public WzImage()
        {
        }

        /// <summary>
        ///   Creates a WzImage with the given name
        /// </summary>
        /// <param name="name"> The name of the image </param>
        public WzImage(string name)
        {
            this.name = name;
        }

        public WzImage(string name, Stream dataStream, WzMapleVersion mapleVersion)
        {
            this.name = name;
            reader = new WzBinaryReader(dataStream, WzTool.GetIvByMapleVersion(mapleVersion));
        }

        internal WzImage(string name, WzBinaryReader reader)
        {
            this.name = name;
            this.reader = reader;
            blockStart = (int) reader.BaseStream.Position;
        }

        public override void Dispose()
        {
            name = null;
            reader = null;
            if (properties != null)
            {
                foreach (IWzImageProperty prop in properties) prop.Dispose();
                properties.Clear();
                properties = null;
            }
        }

        #endregion

        /// <summary>
        ///   The parent of the object
        /// </summary>
        public override IWzObject Parent
        {
            get { return parent; }
            internal set { parent = value; }
        }

        /// <summary>
        ///   The name of the image
        /// </summary>
        public override string Name
        {
            get { return name; }
        }

        public override IWzFile WzFileParent
        {
            get { return Parent.WzFileParent; }
        }

        /// <summary>
        ///   Is the object parsed
        /// </summary>
        public bool Parsed
        {
            get { return parsed; }
            set { parsed = value; }
        }

        /// <summary>
        ///   Was the image changed
        /// </summary>
        public bool Changed
        {
            get { return changed; }
            set { changed = value; }
        }

        /// <summary>
        ///   The size in the wz file of the image
        /// </summary>
        public int BlockSize
        {
            get { return size; }
            set { size = value; }
        }

        /// <summary>
        ///   The checksum of the image
        /// </summary>
        public int Checksum
        {
            get { return checksum; }
            set { checksum = value; }
        }

        /// <summary>
        ///   The offset of the image
        /// </summary>
        public uint Offset
        {
            get { return offset; }
            set { offset = value; }
        }

        public int BlockStart
        {
            get { return blockStart; }
        }

        /// <summary>
        ///   The WzObjectType of the image
        /// </summary>
        public override WzObjectType ObjectType
        {
            get
            {
                if (reader != null) if (!parsed) ParseImage();
                return WzObjectType.Image;
            }
        }

        #region IPropertyContainer Members

        /// <summary>
        ///   The properties contained in the image
        /// </summary>
        public List<IWzImageProperty> WzProperties
        {
            get
            {
                if (reader != null && !parsed)
                {
                    ParseImage();
                }
                return properties;
            }
        }

        /// <summary>
        ///   Gets a wz property by it's name
        /// </summary>
        /// <param name="name"> The name of the property </param>
        /// <returns> The wz property with the specified name </returns>
        public IWzImageProperty this[string name]
        {
            get
            {
                if (reader != null) if (!parsed) ParseImage();
                foreach (IWzImageProperty iwp in properties) if (iwp.Name.ToLower() == name.ToLower()) return iwp;
                return null;
            }
        }

        /// <summary>
        ///   Adds a property to the image
        /// </summary>
        /// <param name="prop"> Property to add </param>
        public void AddProperty(IWzImageProperty prop)
        {
            prop.Parent = this;
            if (reader != null && !parsed) ParseImage();
            properties.Add(prop);
        }

        public void AddProperties(List<IWzImageProperty> props)
        {
            foreach (IWzImageProperty prop in props)
            {
                AddProperty(prop);
            }
        }

        /// <summary>
        ///   Removes a property by name
        /// </summary>
        /// <param name="name"> The name of the property to remove </param>
        public void RemoveProperty(IWzImageProperty prop)
        {
            if (reader != null && !parsed) ParseImage();
            prop.Parent = null;
            properties.Remove(prop);
        }

        public void ClearProperties()
        {
            foreach (IWzImageProperty prop in properties) prop.Parent = null;
            properties.Clear();
        }

        #endregion

        public WzImage DeepClone()
        {
            if (reader != null && !parsed) ParseImage();
            WzImage clone = (WzImage) MemberwiseClone();
            clone.properties = new List<IWzImageProperty>();
            foreach (IWzImageProperty prop in properties) clone.properties.Add(prop.DeepClone());
            return clone;
        }

        /// <summary>
        ///   Gets a WzImageProperty from a path
        /// </summary>
        /// <param name="path"> path to object </param>
        /// <returns> the selected WzImageProperty </returns>
        public IWzImageProperty GetFromPath(string path)
        {
            if (reader != null) if (!parsed) ParseImage();

            string[] segments = path.Split(new char[1] {'/'}, StringSplitOptions.RemoveEmptyEntries);
            if (segments[0] == "..")
            {
                return null;
            }

            //hack method of adding the properties
            WzSubProperty childProperties = new WzSubProperty();
            childProperties.AddProperties(properties);

            IWzImageProperty ret = childProperties;
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

        #region Parsing Methods

        public byte[] DataBlock
        {
            get
            {
                byte[] blockData = null;
                if (reader != null && size > 0)
                {
                    blockData = reader.ReadBytes(size);
                    reader.BaseStream.Position = blockStart;
                }
                return blockData;
            }
        }

        /// <summary>
        ///   Parses the image from the wz filetod
        /// </summary>
        /// <param name="wzReader"> The BinaryReader that is currently reading the wz file </param>
        public void ParseImage(bool parseEverything = false)
        {
            if (Parsed) return;
            else if (Changed)
            {
                Parsed = true;
                return;
            }
            this.parseEverything = parseEverything;
            reader.BaseStream.Position = offset;
            byte b = reader.ReadByte();
            if (b == 0x73)
            {
                long originalPos2 = reader.BaseStream.Position;
                if (reader.ReadWzString() != "Property")
                {
                    isEncrypted = false;
                    reader.BaseStream.Position = originalPos2;
                    if (reader.ReadWzString(false) != "Property") return;
                }
                if (reader.ReadUInt16() != 0) return;
            }
            else return;
            properties.AddRange(IWzImageProperty.ParsePropertyList(offset, reader, this, this, isEncrypted));
            parsed = true;
        }

        public void UnparseImage()
        {
            parsed = false;
            properties = new List<IWzImageProperty>();
        }

        #endregion
    }
}