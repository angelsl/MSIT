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
using System.Linq;
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
            string aWzInPath = null;
            bool aPngOutput = false;
            var aWzVer = (WzMapleVersion) int.MinValue;
            string aOutputPath = null;
            Color aBgColor = Color.Black;
            int aPadding = 10;
            // input, input-wzfile, input-wzpath, input-wzver, output, output-path, background-color, padding
            var set = new OptionSet();
            set.Add("iwzp=|input-wzpath=", "The path of the animation or image. Required", s => aWzInPath = s);
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

            printHelp |= aWzInPath == null || aWzVer == (WzMapleVersion) int.MinValue || aOutputPath == null;
            if (printHelp)
            {
                PrintHelp(set);
                return;
            }

            #endregion

            string[] wzpaths = aWzInPath.Split('*');
            var framess = new List<List<Frame>>();
            foreach(var wzpath in wzpaths)
            {
                string[] split = wzpath.Split('?');
                string path = split[0];
                string inPath = split[1];
                #region wz parsing
                var wz = new WzFile(path, aWzVer);
                wz.ParseWzFile();
                #endregion

                #region getting single image

                var wzcp = wz.GetWzObjectFromPath(inPath) as WzCanvasProperty;
                if (wzcp != null)
                {
                    var b = wzcp.PngProperty.GetPNG(false);
                    b.Save(aOutputPath, aPngOutput ? ImageFormat.Png : ImageFormat.Gif);
                    return;
                }
                #endregion
                try
                {
                    List<Frame> data = InputMethods.InputWz(wz, inPath);
                    framess.Add(data);
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occured while retrieving frames. Check your arguments.");
                    Console.WriteLine(e.ToString());
                }
                wz.Dispose();
            }
            var final = OffsetAnimator.Process(new Rectangle(aPadding, aPadding, aPadding, aPadding), aBgColor, framess.ToArray());
            if (aPngOutput) OutputMethods.OutputPNG(final, aOutputPath);
            else OutputMethods.OutputGIF(final, aOutputPath);
            /*
            #region wz parsing
            var wz = new WzFile(aWzInPath, aWzVer);
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
                data = OffsetAnimator.Process(new Rectangle(aPadding, aPadding, aPadding, aPadding), aBgColor, data);
                if (aPngOutput) OutputMethods.OutputPNG(data, aOutputPath);
                else OutputMethods.OutputGIF(data, aOutputPath);
            } catch(Exception e)
            {
                Console.WriteLine("An error occured while animating. Check your arguments.");
                Console.WriteLine(e.ToString());
            }
            #endregion*/
            //TODO: A LOT OF SHIT

        }

        private static void PrintHelp(OptionSet set)
        {
            Console.WriteLine("Usage: MSIT <options>");
            Console.WriteLine();
            set.WriteOptionDescriptions(Console.Out);
        }
    }
}