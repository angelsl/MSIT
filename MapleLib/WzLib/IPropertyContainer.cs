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
using System.Collections.Generic;

namespace MapleLib.WzLib
{
    public interface IPropertyContainer
    {
        List<IWzImageProperty> WzProperties { get; }
        IWzImageProperty this[string name] { get; }
        void AddProperty(IWzImageProperty prop);
        void AddProperties(List<IWzImageProperty> props);
        void RemoveProperty(IWzImageProperty prop);
        void ClearProperties();
    }
}