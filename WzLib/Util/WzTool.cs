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
using System.Collections;

namespace MSIT.WzLib.Util
{
    public class WzTool
    {
        public const int WzHeader = 0x31474B50; //PKG1

        public static UInt32 RotateLeft(UInt32 x, byte n)
        {
            return (((x) << (n)) | ((x) >> (32 - (n))));
        }

        public static byte[] GetIvByMapleVersion(WzMapleVersion ver)
        {
            switch (ver)
            {
                case WzMapleVersion.MSEA:
                case WzMapleVersion.KMST:
                case WzMapleVersion.KMS:
                case WzMapleVersion.JMS:
                case WzMapleVersion.JMST:
                case WzMapleVersion.EMS:
                    return CryptoConstants.WZMSEAIV;
                case WzMapleVersion.TMS:
                case WzMapleVersion.GMS:
                case WzMapleVersion.GMST:
                    return CryptoConstants.WZGMSIV;
                case WzMapleVersion.BMS:
                case WzMapleVersion.Classic:
                    return new byte[4];
                default:
                    throw new ArgumentException("Invalid WzMapleVersion provided.", "ver");
            }
        }
    }
}