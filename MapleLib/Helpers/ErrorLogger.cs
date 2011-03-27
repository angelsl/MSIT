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
using System.Collections.Generic;

namespace MapleLib.Helpers
{
    public static class ErrorLogger
    {
        private static readonly List<Error> errorList = new List<Error>();

        public static void Log(ErrorLevel level, string message)
        {
            errorList.Add(new Error(level, message));
        }
    }

    internal class Error
    {
        private ErrorLevel level;
        private string message;

        internal Error(ErrorLevel level, string message)
        {
            this.level = level;
            this.message = message;
        }
    }

    public enum ErrorLevel
    {
        MissingFeature,
        IncorrectStructure,
        Critical,
        Crash
    }
}