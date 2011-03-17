// This file is part of MSreinator. This file may have been taken from other applications and libraries.
// 
// MSreinator is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// MSreinator is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MSreinator.  If not, see <http://www.gnu.org/licenses/>.
using MapleLib.WzLib.WzProperties;
//using HaCreator.MapEditor;

namespace MapleLib.WzLib.WzStructure
{
    public static class InfoTool
    {
        public static string GetString(IWzImageProperty source)
        {
            return ((WzStringProperty) source).Value;
        }

        public static double GetDouble(IWzImageProperty source)
        {
            return ((WzDoubleProperty) source).Value;
        }

        public static int GetInt(IWzImageProperty source)
        {
            return ((WzCompressedIntProperty) source).Value;
        }

        public static int? GetOptionalInt(IWzImageProperty source)
        {
            return source == null ? (int?) null : ((WzCompressedIntProperty) source).Value;
        }


        public static MapleBool GetOptionalBool(IWzImageProperty source)
        {
            if (source == null) return MapleBool.NotExist;
            else return ((WzCompressedIntProperty) source).Value == 1;
        }

        public static bool GetBool(IWzImageProperty source)
        {
            return ((WzCompressedIntProperty) source).Value == 1;
        }

        public static float GetFloat(IWzImageProperty source)
        {
            return ((WzByteFloatProperty) source).Value;
        }

        public static int? GetOptionalTranslatedInt(IWzImageProperty source)
        {
            string str = GetOptionalString(source);
            if (str == null) return null;
            return int.Parse(str);
        }

        public static string GetOptionalString(IWzImageProperty source)
        {
            return source == null ? null : ((WzStringProperty) source).Value;
        }
    }
}