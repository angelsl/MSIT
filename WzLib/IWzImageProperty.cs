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
using MSIT.WzLib.Util;
using MSIT.WzLib.WzProperties;

namespace MSIT.WzLib
{
    /// <summary>
    ///   An interface for wz img properties
    /// </summary>
    public abstract class IWzImageProperty : IWzObject
    {
        public virtual List<IWzImageProperty> WzProperties
        {
            get { return null; }
        }

        public virtual IWzImageProperty this[string name]
        {
            get { return null; }
        }


        public abstract WzPropertyType PropertyType { get; }

        /// <summary>
        ///   The image that this property is contained in
        /// </summary>
        public WzImage ParentImage
        {
            get
            {
                IWzObject parent = Parent;
                while (parent != null)
                {
                    if (parent is WzImage) return (WzImage) parent;
                    else parent = parent.Parent;
                }
                return null;
            }
        }

        public override WzObjectType ObjectType
        {
            get { return WzObjectType.Property; }
        }

        public override IWzFile WzFileParent
        {
            get { return ParentImage.WzFileParent; }
        }

        #region Extended Properties Parsing

        internal static List<IWzImageProperty> ParsePropertyList(uint offset, WzBinaryReader reader, IWzObject parent, WzImage parentImg, bool enc)
        {
            int entryCount = reader.ReadCompressedInt();
            List<IWzImageProperty> properties = new List<IWzImageProperty>(entryCount);
            for (int i = 0; i < entryCount; i++)
            {
                string name = reader.ReadStringBlock(offset, enc);
                switch (reader.ReadByte())
                {
                    case 0:
                        properties.Add(new WzNullProperty(name) {Parent = parent /*, ParentImage = parentImg*/});
                        break;
                    case 0x0B:
                    case 2:
                        properties.Add(new WzUnsignedShortProperty(name, reader.ReadUInt16()) {Parent = parent /*, ParentImage = parentImg*/});
                        break;
                    case 3:
                        properties.Add(new WzCompressedIntProperty(name, reader.ReadCompressedInt()) {Parent = parent /*, ParentImage = parentImg*/});
                        break;
                    case 4:
                        byte type = reader.ReadByte();
                        if (type == 0x80) properties.Add(new WzByteFloatProperty(name, reader.ReadSingle()) {Parent = parent /*, ParentImage = parentImg*/});
                        else if (type == 0) properties.Add(new WzByteFloatProperty(name, 0f) {Parent = parent /*, ParentImage = parentImg*/});
                        break;
                    case 5:
                        properties.Add(new WzDoubleProperty(name, reader.ReadDouble()) {Parent = parent /*, ParentImage = parentImg*/});
                        break;
                    case 8:
                        properties.Add(new WzStringProperty(name, reader.ReadStringBlock(offset, enc)) {Parent = parent});
                        break;
                    case 9:
                        int eob = (int) (reader.ReadUInt32() + reader.BaseStream.Position);
                        IWzImageProperty exProp = ParseExtendedProp(reader, offset, eob, name, parent, parentImg, enc);
                        properties.Add(exProp);
                        reader.BaseStream.Position = eob;
                        break;
                    default:
                        throw new Exception("Unknown property type at ParsePropertyList");
                }
            }
            return properties;
        }

        private static Extended ParseExtendedProp(WzBinaryReader reader, uint offset, int endOfBlock, string name, IWzObject parent, WzImage imgParent, bool enc)
        {
            switch (reader.ReadByte())
            {
                case 0x1B:
                    return ExtractMore(reader, offset, endOfBlock, name, reader.ReadStringAtOffset(offset + reader.ReadInt32()), parent, imgParent, enc);
                case 0x73:
                    return ExtractMore(reader, offset, endOfBlock, name, "", parent, imgParent, enc);
                default:
                    throw new Exception("Invlid byte read at ParseExtendedProp");
            }
        }

        private static Extended ExtractMore(WzBinaryReader reader, uint offset, int eob, string name, string iname, IWzObject parent, WzImage imgParent, bool enc)
        {
            if (iname == "") iname = reader.ReadWzString(enc);
            switch (iname)
            {
                case "Property":
                    WzSubProperty subProp = new WzSubProperty(name) {Parent = parent};
                    reader.BaseStream.Position += 2;
                    subProp.AddProperties(ParsePropertyList(offset, reader, subProp, imgParent, enc));
                    return subProp;
                case "Canvas":
                    WzCanvasProperty canvasProp = new WzCanvasProperty(name) {Parent = parent};
                    reader.BaseStream.Position++;
                    if (reader.ReadByte() == 1)
                    {
                        reader.BaseStream.Position += 2;
                        canvasProp.AddProperties(ParsePropertyList(offset, reader, canvasProp, imgParent, enc));
                    }
                    canvasProp.PngProperty = new WzPngProperty(reader, imgParent.parseEverything, enc) {Parent = canvasProp};
                    return canvasProp;
                case "Shape2D#Vector2D":
                    WzVectorProperty vecProp = new WzVectorProperty(name) {Parent = parent};
                    vecProp.X = new WzCompressedIntProperty("X", reader.ReadCompressedInt()) {Parent = vecProp};
                    vecProp.Y = new WzCompressedIntProperty("Y", reader.ReadCompressedInt()) {Parent = vecProp};
                    return vecProp;
                case "Shape2D#Convex2D":
                    WzConvexProperty convexProp = new WzConvexProperty(name) {Parent = parent};
                    int convexEntryCount = reader.ReadCompressedInt();
                    convexProp.WzProperties.Capacity = convexEntryCount; //performance thing
                    for (int i = 0; i < convexEntryCount; i++)
                    {
                        convexProp.AddProperty(ParseExtendedProp(reader, offset, 0, name, convexProp, imgParent, enc));
                    }
                    return convexProp;
                case "Sound_DX8":
                    WzSoundProperty soundProp = new WzSoundProperty(name, reader, imgParent.parseEverything) {Parent = parent};
                    return soundProp;
                case "UOL":
                    reader.BaseStream.Position++;
                    switch (reader.ReadByte())
                    {
                        case 0:
                            return new WzUOLProperty(name, reader.ReadWzString(enc)) {Parent = parent};
                        case 1:
                            return new WzUOLProperty(name, reader.ReadStringAtOffset(offset + reader.ReadInt32(), false, true)) {Parent = parent};
                    }
                    throw new Exception("Unsupported UOL type");
                default:
                    throw new Exception("Unknown iname: " + iname);
            }
        }

        #endregion

        public virtual IWzImageProperty GetFromPath(string path)
        {
            return null;
        }

        public abstract IWzImageProperty DeepClone();
    }
}