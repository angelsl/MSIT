﻿// This file is part of MSreinator. This file may have been taken from other applications and libraries.
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
namespace MapleLib.WzLib.WzStructure.Data
{
    public static class Tables
    {
        public static string[] PortalTypeNames = new[]
                                                     {
                                                         "Start Point", "Invisible", "Visible", "Collision", "Changable", "Changable Invisible", "Town Portal", "Script", "Script Invisible",
                                                         "Script Collision", "Hidden", "Script Hidden", "Vertical Spring", "Custom Impact Spring", "Unknown (PCIG)"
                                                     };

        public static string[] BackgroundTypeNames = new[]
                                                         {
                                                             "Regular", "Horizontal Copies", "Vertical Copies", "H+V Copies", "Horizontal Moving+Copies", "Vertical Moving+Copies",
                                                             "H+V Copies, Horizontal Moving", "H+V Copies, Vertical Moving"
                                                         };
    }

    public enum QuestState
    {
        Available = 0,
        InProgress = 1,
        Completed = 2
    }
}