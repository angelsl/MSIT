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

namespace MSIT.WzLib
{
    /// <summary>
    ///   Contains all the constant values used for various functions
    /// </summary>
    public static class CryptoConstants
    {
        /// <summary>
        ///   Constant used in WZ offset encryption
        /// </summary>
        public const uint WZOffsetConstant = 0x581C3F6D;

        /// <summary>
        ///   AES UserKey used by MapleStory
        /// </summary>
        public static readonly byte[] UserKey = new byte[] {0x13, 0x00, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00, 0x06, 0x00, 0x00, 0x00, 0xB4, 0x00, 0x00, 0x00, 0x1B, 0x00, 0x00, 0x00, 0x0F, 0x00, 0x00, 0x00, 0x33, 0x00, 0x00, 0x00, 0x52, 0x00, 0x00, 0x00};

        /// <summary>
        ///   IV used to create the WzKey for GMS
        /// </summary>
        public static readonly byte[] WZGMSIV = new byte[4] {0x4D, 0x23, 0xC7, 0x2B};

        /// <summary>
        ///   IV used to create the WzKey for MSEA
        /// </summary>
        public static readonly byte[] WZMSEAIV = new byte[4] {0xB9, 0x7D, 0x63, 0xE9};

    }
}