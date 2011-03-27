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
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using MapleLib.WzLib;
using MapleLib.WzLib.WzProperties;
using Mono.Options;

namespace MSIT
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            #region getopt

            bool printHelp = false;
            string aWzFilePath = null;
            string aWzInPath = null;
            bool aPngOutput = false;
            var aWzVer = (WzMapleVersion) int.MinValue;
            string aOutputPath = null;
            Color aBgColor = Color.Black;
            int aPadding = 10;
            // input, input-wzfile, input-wzpath, input-wzver, output, output-path, background-color, padding
            var set = new OptionSet();
            set.Add("iwzf=|input-wzfile=", "The path of the WZ file to get image(s) from. Required", s => aWzFilePath = s);
            set.Add("iwzp=|input-wzpath=", "The path of the animation or image in the WZ file. Required", s => aWzInPath = s);
            set.Add("iwzv=|input-wzver=", "The WZ key to use when decoding the WZ. Required", s => aWzVer = (WzMapleVersion) Enum.Parse(typeof (WzMapleVersion), s));
            set.Add("o=|output=",
                    "The method of output: (a)png or gif",
                    s =>
                        {
                            switch (s.ToLower())
                            {
                                case "png":
                                    aPngOutput = true;
                                    break;
                                case "gif":
                                    aPngOutput = false;
                                    break;
                                default:
                                    throw new ArgumentException("output must be either png or gif");
                            }
                        });
            set.Add("op=|output-path=", "The path to write the output, (A)PNG or GIF, to", s => aOutputPath = s);
            set.Add("abg=|a-background-color=", "The background color of the animated output. Default is black. Ignored if /animated is not set.", s => aBgColor = Color.FromArgb(int.Parse(s)));
            set.Add("ap=|a-padding=", "The amount of padding in pixels to pad the animated output with. Default is 10. Ignored if /animated is not set.", s => aPadding = int.Parse(s));
            set.Add("?|h|help", "Shows help", s => printHelp = true);
            set.Parse(args);
            #endregion

            #region check params

            printHelp |= aWzFilePath == null || aWzInPath == null || aWzVer == (WzMapleVersion) int.MinValue || aOutputPath == null;
            if (printHelp)
            {
                PrintHelp(set);
                return;
            }

            #endregion

            #region wz parsing
            var wz = new WzFile(aWzFilePath, aWzVer);
            wz.ParseWzFile();
            #endregion

            #region getting single image

            WzCanvasProperty wzcp = wz.GetWzObjectFromPath(aWzInPath) as WzCanvasProperty;
            if(wzcp != null)
            {
                // it's a single image.
                var b = wzcp.PngProperty.GetPNG(false);
                b.Save(aOutputPath, aPngOutput ? ImageFormat.Png : ImageFormat.Gif);
                return;
            }
            #endregion
            #region animations
            try
            {
                IEnumerable<Frame> data = InputMethods.InputWz(wz, aWzInPath);
                data = OffsetAnimator.Process(data, new Rectangle(aPadding, aPadding, aPadding, aPadding), aBgColor);
                if (aPngOutput) OutputMethods.OutputPNG(data, aOutputPath);
                else OutputMethods.OutputGIF(data, aOutputPath);
            } catch(Exception e)
            {
                Console.WriteLine("An error occured while animating. Check your arguments.");
                Console.WriteLine(e.ToString());
            }

            #endregion

        }

        private static void PrintHelp(OptionSet set)
        {
            Console.WriteLine("Usage: MSreinator <options>");
            Console.WriteLine();
            set.WriteOptionDescriptions(Console.Out);
        }
    }
}