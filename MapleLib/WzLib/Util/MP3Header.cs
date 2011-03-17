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
using System.IO;

public class MP3Header
{
    // Public variables for storing the information about the MP3
    private ulong bithdr;
    private bool boolVBitRate;
    public int intBitRate;
    public int intFrequency;
    public int intLength;
    private int intVFrames;
    public long lngFileSize;
    public string strFileName;
    public string strLengthFormatted;
    public string strMode;

    public bool ReadMP3Information(string FileName)
    {
        var fs = new FileStream(FileName, FileMode.Open, FileAccess.Read);
        // Set the filename not including the path information
        strFileName = @fs.Name;
        var chrSeparators = new[] {'\\', '/'};
        string[] strSeparator = strFileName.Split(chrSeparators);
        int intUpper = strSeparator.GetUpperBound(0);
        strFileName = strSeparator[intUpper];

        // Replace ' with '' for the SQL INSERT statement
        strFileName = strFileName.Replace("'", "''");

        // Set the file size
        lngFileSize = fs.Length;

        var bytHeader = new byte[4];
        var bytVBitRate = new byte[12];
        int intPos = 0;

        // Keep reading 4 bytes from the header until we know for sure that in 
        // fact it's an MP3
        do
        {
            fs.Position = intPos;
            fs.Read(bytHeader, 0, 4);
            intPos++;
            LoadMP3Header(bytHeader);
        } while (!IsValidHeader() && (fs.Position != fs.Length));

        // If the current file stream position is equal to the length, 
        // that means that we've read the entire file and it's not a valid MP3 file
        if (fs.Position != fs.Length)
        {
            intPos += 3;

            if (getVersionIndex() == 3) // MPEG Version 1
            {
                if (getModeIndex() == 3) // Single Channel
                {
                    intPos += 17;
                }
                else
                {
                    intPos += 32;
                }
            }
            else // MPEG Version 2.0 or 2.5
            {
                if (getModeIndex() == 3) // Single Channel
                {
                    intPos += 9;
                }
                else
                {
                    intPos += 17;
                }
            }

            // Check to see if the MP3 has a variable bitrate
            fs.Position = intPos;
            fs.Read(bytVBitRate, 0, 12);
            boolVBitRate = LoadVBRHeader(bytVBitRate);

            // Once the file's read in, then assign the properties of the file to the public variables
            intBitRate = getBitrate();
            intFrequency = getFrequency();
            strMode = getMode();
            intLength = getLengthInSeconds();
            strLengthFormatted = getFormattedLength();
            fs.Close();
            return true;
        }
        return false;
    }

    private void LoadMP3Header(byte[] c)
    {
        // this thing is quite interesting, it works like the following
        // c[0] = 00000011
        // c[1] = 00001100
        // c[2] = 00110000
        // c[3] = 11000000
        // the operator << means that we'll move the bits in that direction
        // 00000011 << 24 = 00000011000000000000000000000000
        // 00001100 << 16 =         000011000000000000000000
        // 00110000 << 24 =                 0011000000000000
        // 11000000       =                         11000000
        //                +_________________________________
        //                  00000011000011000011000011000000
        bithdr = (ulong) (((c[0] & 255) << 24) | ((c[1] & 255) << 16) | ((c[2] & 255) << 8) | ((c[3] & 255)));
    }

    private bool LoadVBRHeader(byte[] inputheader)
    {
        // If it's a variable bitrate MP3, the first 4 bytes will read 'Xing'
        // since they're the ones who added variable bitrate-edness to MP3s
        if (inputheader[0] == 88 && inputheader[1] == 105 && inputheader[2] == 110 && inputheader[3] == 103)
        {
            int flags = (((inputheader[4] & 255) << 24) | ((inputheader[5] & 255) << 16) | ((inputheader[6] & 255) << 8) | ((inputheader[7] & 255)));
            if ((flags & 0x0001) == 1)
            {
                intVFrames = (((inputheader[8] & 255) << 24) | ((inputheader[9] & 255) << 16) | ((inputheader[10] & 255) << 8) | ((inputheader[11] & 255)));
                return true;
            }
            else
            {
                intVFrames = -1;
                return true;
            }
        }
        return false;
    }

    private bool IsValidHeader()
    {
        return (((getFrameSync() & 2047) == 2047) && ((getVersionIndex() & 3) != 1) && ((getLayerIndex() & 3) != 0) && ((getBitrateIndex() & 15) != 0) && ((getBitrateIndex() & 15) != 15) &&
                ((getFrequencyIndex() & 3) != 3) && ((getEmphasisIndex() & 3) != 2));
    }

    private int getFrameSync()
    {
        return (int) ((bithdr >> 21) & 2047);
    }

    private int getVersionIndex()
    {
        return (int) ((bithdr >> 19) & 3);
    }

    private int getLayerIndex()
    {
        return (int) ((bithdr >> 17) & 3);
    }

    private int getProtectionBit()
    {
        return (int) ((bithdr >> 16) & 1);
    }

    private int getBitrateIndex()
    {
        return (int) ((bithdr >> 12) & 15);
    }

    private int getFrequencyIndex()
    {
        return (int) ((bithdr >> 10) & 3);
    }

    private int getPaddingBit()
    {
        return (int) ((bithdr >> 9) & 1);
    }

    private int getPrivateBit()
    {
        return (int) ((bithdr >> 8) & 1);
    }

    private int getModeIndex()
    {
        return (int) ((bithdr >> 6) & 3);
    }

    private int getModeExtIndex()
    {
        return (int) ((bithdr >> 4) & 3);
    }

    private int getCoprightBit()
    {
        return (int) ((bithdr >> 3) & 1);
    }

    private int getOrginalBit()
    {
        return (int) ((bithdr >> 2) & 1);
    }

    private int getEmphasisIndex()
    {
        return (int) (bithdr & 3);
    }

    private double getVersion()
    {
        double[] table = {2.5, 0.0, 2.0, 1.0};
        return table[getVersionIndex()];
    }

    private int getLayer()
    {
        return (4 - getLayerIndex());
    }

    private int getBitrate()
    {
        // If the file has a variable bitrate, then we return an integer average bitrate,
        // otherwise, we use a lookup table to return the bitrate
        if (boolVBitRate)
        {
            double medFrameSize = lngFileSize/(double) getNumberOfFrames();
            return (int) ((medFrameSize*getFrequency())/(1000.0*((getLayerIndex() == 3) ? 12.0 : 144.0)));
        }
        else
        {
            int[,,] table = {
                                {
// MPEG 2 & 2.5
                                    {0, 8, 16, 24, 32, 40, 48, 56, 64, 80, 96, 112, 128, 144, 160, 0}, // Layer III
                                    {0, 8, 16, 24, 32, 40, 48, 56, 64, 80, 96, 112, 128, 144, 160, 0}, // Layer II
                                    {0, 32, 48, 56, 64, 80, 96, 112, 128, 144, 160, 176, 192, 224, 256, 0} // Layer I
                                }, {
// MPEG 1
                                       {0, 32, 40, 48, 56, 64, 80, 96, 112, 128, 160, 192, 224, 256, 320, 0}, // Layer III
                                       {0, 32, 48, 56, 64, 80, 96, 112, 128, 160, 192, 224, 256, 320, 384, 0}, // Layer II
                                       {0, 32, 64, 96, 128, 160, 192, 224, 256, 288, 320, 352, 384, 416, 448, 0} // Layer I
                                   }
                            };

            return table[getVersionIndex() & 1, getLayerIndex() - 1, getBitrateIndex()];
        }
    }

    private int getFrequency()
    {
        int[,] table = {
                           {32000, 16000, 8000}, // MPEG 2.5
                           {0, 0, 0}, // reserved
                           {22050, 24000, 16000}, // MPEG 2
                           {44100, 48000, 32000} // MPEG 1
                       };

        return table[getVersionIndex(), getFrequencyIndex()];
    }

    private string getMode()
    {
        switch (getModeIndex())
        {
            default:
                return "Stereo";
            case 1:
                return "Joint Stereo";
            case 2:
                return "Dual Channel";
            case 3:
                return "Single Channel";
        }
    }

    private int getLengthInSeconds()
    {
        // "intKilBitFileSize" made by dividing by 1000 in order to match the "Kilobits/second"
        var intKiloBitFileSize = (int) ((8*lngFileSize)/1000);
        return (intKiloBitFileSize/getBitrate());
    }

    private string getFormattedLength()
    {
        // Complete number of seconds
        int s = getLengthInSeconds();

        // Seconds to display
        int ss = s%60;

        // Complete number of minutes
        int m = (s - ss)/60;

        // Minutes to display
        int mm = m%60;

        // Complete number of hours
        int h = (m - mm)/60;

        // Make "hh:mm:ss"
        return h.ToString("D2") + ":" + mm.ToString("D2") + ":" + ss.ToString("D2");
    }

    private int getNumberOfFrames()
    {
        // Again, the number of MPEG frames is dependant on whether it's a variable bitrate MP3 or not
        if (!boolVBitRate)
        {
            double medFrameSize = (((getLayerIndex() == 3) ? 12 : 144)*((1000.0*getBitrate())/getFrequency()));
            return (int) (lngFileSize/medFrameSize);
        }
        else
            return intVFrames;
    }
}