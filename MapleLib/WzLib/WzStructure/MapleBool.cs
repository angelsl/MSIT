﻿/*  MapleLib - A general-purpose MapleStory library
 * Copyright (C) 2009, 2010 Snow and haha01haha01
   
 * This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

 * This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

 * You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapleLib.WzLib.WzStructure
{
    public struct MapleBool //I know I could have used the nullable bool.
    {
        public static readonly byte NotExist = 0;
        public static readonly byte False = 1;
        public static readonly byte True = 2;

        private byte val { get; set; }
        public static implicit operator MapleBool(byte value)
        {
            return new MapleBool
            {
                val = value
            };
        }

        public static implicit operator MapleBool(bool? value)
        {
            return new MapleBool
            {
                val = value == null ? MapleBool.NotExist : (bool)value ? MapleBool.True : MapleBool.False
            };
        }

        public static implicit operator bool(MapleBool value)
        {
            return value == MapleBool.True;
        }

        public override bool Equals(object obj)
        {
            return obj is MapleBool ? ((MapleBool)obj).val.Equals(val) : false;
        }

        public override int GetHashCode()
        {
            return val.GetHashCode();
        }

        public static bool operator ==(MapleBool a, MapleBool b)
        {
            return a.val.Equals(b.val);
        }

        public static bool operator ==(MapleBool a, bool b)
        {
            return (b && (a.val == MapleBool.True)) || (!b && (a.val == MapleBool.False));
        }

        public static bool operator !=(MapleBool a, MapleBool b)
        {
            return !a.val.Equals(b.val);
        }

        public static bool operator !=(MapleBool a, bool b)
        {
            return (b && (a.val != MapleBool.True)) || (!b && (a.val != MapleBool.False));
        }
    }
}
