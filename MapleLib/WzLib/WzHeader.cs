﻿// This file is part of MSIT. This file may have been taken from other applications and libraries.
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
namespace MapleLib.WzLib
{
    public class WzHeader
    {
        private string copyright;
        private ulong fsize;
        private uint fstart;
        private string ident;

        public string Ident
        {
            get { return ident; }
            set { ident = value; }
        }

        public string Copyright
        {
            get { return copyright; }
            set { copyright = value; }
        }

        public ulong FSize
        {
            get { return fsize; }
            set { fsize = value; }
        }

        public uint FStart
        {
            get { return fstart; }
            set { fstart = value; }
        }

        public void RecalculateFileStart()
        {
            fstart = (uint) (ident.Length + sizeof (ulong) + sizeof (uint) + copyright.Length + 1);
        }

        public static WzHeader GetDefault()
        {
            var header = new WzHeader();
            header.ident = "PKG1";
            header.copyright = "Package file v1.0 Copyright 2002 Wizet, ZMS";
            header.fstart = 60;
            header.fsize = 0;
            return header;
        }
    }
}