// This file is part of MSIT. This file may have been taken from other applications and libraries.
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
namespace MapleLib.WzLib.WzStructure
{
    public struct Foothold
    {
        public int layer;
        public int next;
        public int num;
        public int prev;
        public int x1, x2, y1, y2;

        public Foothold(int x1, int x2, int y1, int y2, int num, int layer)
        {
            this.x1 = x1;
            this.x2 = x2;
            this.y1 = y1;
            this.y2 = y2;
            next = 0;
            prev = 0;
            this.num = num;
            this.layer = layer;
        }

        public bool IsWall()
        {
            return x1 == x2;
        }
    }
}