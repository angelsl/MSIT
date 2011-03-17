using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapleLib.Helpers
{
    public static class ErrorLogger
    {
        private static List<Error> errorList = new List<Error>();
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
