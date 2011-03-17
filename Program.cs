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
using System;
using System.Collections.Generic;
using System.Drawing;
using MapleLib.WzLib;
using Mono.Options;

namespace MSreinator
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            #region getopt

            bool printHelp = false;
            bool aWzInput = true;
            string aWzFilePath = null;
            string aWzInPath = null;
            bool aApngOutput = false;
            var aWzVer = (WzMapleVersion) int.MinValue;
            string aOutputPath = null;
            Color aBgColor = Color.Black;
            int aPadding = 10;
            // input, input-wzfile, input-wzpath, input-wzver, output, output-path, background-color, padding
            var set = new OptionSet();
            set.Add("i=|input=",
                    "The method of input: via a WZ path or directly on the command line. Valid options are \"wz\" and \"args\". Default is wz",
                    s =>
                        {
                            switch (s.ToLower())
                            {
                                case "wz":
                                    aWzInput = true;
                                    break;
                                case "args":
                                    aWzInput = false;
                                    break;
                                default:
                                    throw new ArgumentException("input must be either wz or args");
                            }
                        });
            set.Add("iwzf=|input-wzfile=", "The path of the WZ file to get images from. Required if input:wz", s => aWzFilePath = s);
            set.Add("iwzp=|input-wzpath=", "The path of the animation in the WZ file. Required if input:wz", s => aWzInPath = s);
            set.Add("iwzv=|input-wzver=", "The WZ key to use when decoding the WZ. Required if input:wz", s => aWzVer = (WzMapleVersion) Enum.Parse(typeof (WzMapleVersion), s));
            set.Add("o=|output=",
                    "The method of output: apng or gif",
                    s =>
                        {
                            switch (s.ToLower())
                            {
                                case "apng":
                                    aApngOutput = true;
                                    break;
                                case "gif":
                                    aApngOutput = false;
                                    break;
                                default:
                                    throw new ArgumentException("output must be either apng or gif");
                            }
                        });
            set.Add("op=|output-path=", "The path to write the output, APNG or GIF, to", s => aOutputPath = s);
            set.Add("bg=|background-color=", "The background color of the output. Default is black", s => aBgColor = Color.FromArgb(int.Parse(s)));
            set.Add("p=|padding=", "The amount of padding in pixels to pad the output with. Default is 10", s => aPadding = int.Parse(s));
            set.Add("?|h|help", "Shows help", s => printHelp = true);
            List<string> unparsed = set.Parse(args);

            #endregion

            #region check params

            printHelp |= (aWzInput && (aWzFilePath == null || aWzInPath == null || aWzVer == (WzMapleVersion) int.MinValue)) || aOutputPath == null;
            if (printHelp)
            {
                PrintHelp(set);
                return;
            }

            #endregion

            IEnumerable<Frame> data = aWzInput ? InputMethods.InputWz(aWzFilePath, aWzInPath, aWzVer) : InputMethods.InputArgs(unparsed);
            data = OffsetAnimator.Process(data, new Rectangle(aPadding, aPadding, aPadding, aPadding), aBgColor);
            if (aApngOutput) OutputMethods.OutputAPNG(data, aOutputPath);
            else OutputMethods.OutputGIF(data, aOutputPath);
        }

        private static void PrintHelp(OptionSet set)
        {
            Console.WriteLine("Usage: MSreinator <options> [data for /input:args]");
            Console.WriteLine();
            set.WriteOptionDescriptions(Console.Out);
        }
    }
}