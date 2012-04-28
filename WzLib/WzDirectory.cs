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

using System.Collections.Generic;
using System.IO;
using MSIT.WzLib.Util;

namespace MSIT.WzLib
{
    /// <summary>
    ///   A directory in the wz file, which may contain sub directories or wz images
    /// </summary>
    public class WzDirectory : IWzObject
    {
        #region Fields

        internal byte[] WzIv;
        internal int checksum;
        internal uint hash;
        internal List<WzImage> images = new List<WzImage>();
        internal string name;
        internal uint offset;
        internal int offsetSize;
        internal IWzObject parent;
        internal WzBinaryReader reader;
        internal int size;
        internal List<WzDirectory> subDirs = new List<WzDirectory>();
        internal WzFile wzFile;

        #endregion

        /// <summary>
        ///   Creates a blank WzDirectory
        /// </summary>
        public WzDirectory()
        {
        }

        /// <summary>
        ///   Creates a WzDirectory with the given name
        /// </summary>
        /// <param name="name"> The name of the directory </param>
        public WzDirectory(string name)
        {
            this.name = name;
        }

        /// <summary>
        ///   Creates a WzDirectory
        /// </summary>
        /// <param name="reader"> The BinaryReader that is currently reading the wz file </param>
        /// <param name="blockStart"> The start of the data block </param>
        /// <param name="parentname"> The name of the directory </param>
        /// <param name="wzFile"> The parent Wz File </param>
        internal WzDirectory(WzBinaryReader reader, string dirName, uint verHash, byte[] WzIv, WzFile wzFile)
        {
            this.reader = reader;
            name = dirName;
            hash = verHash;
            this.WzIv = WzIv;
            this.wzFile = wzFile;
        }

        /// <summary>
        ///   The parent of the object
        /// </summary>
        public override IWzObject Parent
        {
            get { return parent; }
            internal set { parent = value; }
        }

        /// <summary>
        ///   The name of the directory
        /// </summary>
        public override string Name
        {
            get { return name; }
        }

        /// <summary>
        ///   The WzObjectType of the directory
        /// </summary>
        public override WzObjectType ObjectType
        {
            get { return WzObjectType.Directory; }
        }

        public override IWzFile WzFileParent
        {
            get { return wzFile; }
        }

        /// <summary>
        ///   The size of the directory in the wz file
        /// </summary>
        public int BlockSize
        {
            get { return size; }
            set { size = value; }
        }

        /// <summary>
        ///   The directory's chceksum
        /// </summary>
        public int Checksum
        {
            get { return checksum; }
            set { checksum = value; }
        }

        /// <summary>
        ///   The wz images contained in the directory
        /// </summary>
        public List<WzImage> WzImages
        {
            get { return images; }
        }

        /// <summary>
        ///   The sub directories contained in the directory
        /// </summary>
        public List<WzDirectory> WzDirectories
        {
            get { return subDirs; }
        }

        /// <summary>
        ///   Offset of the folder
        /// </summary>
        public uint Offset
        {
            get { return offset; }
            set { offset = value; }
        }

        /// <summary>
        ///   Returns a WzImage or a WzDirectory with the given name
        /// </summary>
        /// <param name="name"> The name of the img or dir to find </param>
        /// <returns> A WzImage or WzDirectory </returns>
        public IWzObject this[string name]
        {
            get
            {
                foreach (WzImage i in images) if (i.Name.ToLower() == name.ToLower()) return i;
                foreach (WzDirectory d in subDirs) if (d.Name.ToLower() == name.ToLower()) return d;
                //throw new KeyNotFoundException("No wz image or directory was found with the specified name");
                return null;
            }
        }

        /// <summary>
        ///   Disposes the obejct
        /// </summary>
        public override void Dispose()
        {
            name = null;
            reader = null;
            foreach (WzImage img in images) img.Dispose();
            foreach (WzDirectory dir in subDirs) dir.Dispose();
            images.Clear();
            subDirs.Clear();
            images = null;
            subDirs = null;
        }


        /// <summary>
        ///   Parses the WzDirectory
        /// </summary>
        internal void ParseDirectory()
        {
            int entryCount = reader.ReadCompressedInt();
            for (int i = 0; i < entryCount; i++)
            {
                byte type = reader.ReadByte();
                string fname = null;
                int fsize;
                int checksum;
                uint offset;

                long rememberPos = 0;
                if (type == 1) //01 XX 00 00 00 00 00 OFFSET (4 bytes) 
                {
                    reader.ReadBytes(10);
                    continue;
                }
                else if (type == 2)
                {
                    int stringOffset = reader.ReadInt32();
                    rememberPos = reader.BaseStream.Position;
                    reader.BaseStream.Position = reader.Header.FStart + stringOffset;
                    type = reader.ReadByte();
                    fname = reader.ReadWzString();
                }
                else if (type == 3 || type == 4)
                {
                    fname = reader.ReadWzString();
                    rememberPos = reader.BaseStream.Position;
                }
                reader.BaseStream.Position = rememberPos;
                fsize = reader.ReadCompressedInt();
                checksum = reader.ReadCompressedInt();
                offset = reader.ReadOffset();
                if (type == 3)
                {
                    WzDirectory subDir = new WzDirectory(reader, fname, hash, WzIv, wzFile);
                    subDir.BlockSize = fsize;
                    subDir.Checksum = checksum;
                    subDir.Offset = offset;
                    subDir.Parent = this;
                    subDirs.Add(subDir);
                }
                else
                {
                    WzImage img = new WzImage(fname, reader);
                    img.BlockSize = fsize;
                    img.Checksum = checksum;
                    img.Offset = offset;
                    img.Parent = this;
                    images.Add(img);
                }
            }

            foreach (WzDirectory subdir in subDirs)
            {
                reader.BaseStream.Position = subdir.offset;
                subdir.ParseDirectory();
            }
        }

        internal void SaveImages(BinaryWriter wzWriter, FileStream fs)
        {
            foreach (WzImage img in images)
            {
                if (img.changed)
                {
                    fs.Position = img.tempFileStart;
                    byte[] buffer = new byte[img.size];
                    fs.Read(buffer, 0, img.size);
                    wzWriter.Write(buffer);
                }
                else
                {
                    img.reader.BaseStream.Position = img.tempFileStart;
                    wzWriter.Write(img.reader.ReadBytes((int) (img.tempFileEnd - img.tempFileStart)));
                }
            }
            foreach (WzDirectory dir in subDirs) dir.SaveImages(wzWriter, fs);
        }

        /// <summary>
        ///   Gets an image in the list of images by it's name
        /// </summary>
        /// <param name="name"> The name of the image </param>
        /// <returns> The wz image that has the specified name or null if none was found </returns>
        public WzImage GetImageByName(string name)
        {
            foreach (WzImage wzI in images) if (wzI.Name.ToLower() == name.ToLower()) return wzI;
            return null;
        }

        /// <summary>
        ///   Gets a sub directory in the list of directories by it's name
        /// </summary>
        /// <param name="name"> The name of the directory </param>
        /// <returns> The wz directory that has the specified name or null if none was found </returns>
        public WzDirectory GetDirectoryByName(string name)
        {
            foreach (WzDirectory dir in subDirs) if (dir.Name.ToLower() == name.ToLower()) return dir;
            return null;
        }

        /// <summary>
        ///   Gets all child images of a WzDirectory
        /// </summary>
        /// <returns> </returns>
        public List<WzImage> GetChildImages()
        {
            List<WzImage> imgFiles = new List<WzImage>();
            imgFiles.AddRange(images);
            foreach (WzDirectory subDir in subDirs)
            {
                imgFiles.AddRange(subDir.GetChildImages());
            }
            return imgFiles;
        }

        /// <summary>
        ///   Removes a sub directory from the list
        /// </summary>
        /// <param name="name"> The sub directory to remove </param>
        public void RemoveDirectory(WzDirectory dir)
        {
            subDirs.Remove(dir);
            dir.Parent = null;
        }

        public WzDirectory DeepClone()
        {
            WzDirectory result = (WzDirectory) MemberwiseClone();
            result.WzDirectories.Clear();
            result.WzImages.Clear();
            foreach (WzDirectory dir in WzDirectories) result.WzDirectories.Add(dir.DeepClone());
            foreach (WzImage img in WzImages) result.WzImages.Add(img.DeepClone());
            return result;
        }

        public int CountImages()
        {
            int result = images.Count;
            foreach (WzDirectory subdir in WzDirectories) result += subdir.CountImages();
            return result;
        }
    }
}