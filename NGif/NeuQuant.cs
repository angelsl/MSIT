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

#region Java

/* NeuQuant Neural-Net Quantization Algorithm
 * ------------------------------------------
 *
 * Copyright (c) 1994 Anthony Dekker
 *
 * NEUQUANT Neural-Net quantization algorithm by Anthony Dekker, 1994.
 * See "Kohonen neural networks for optimal colour quantization"
 * in "Network: Computation in Neural Systems" Vol. 5 (1994) pp 351-367.
 * for a discussion of the algorithm.
 *
 * Any party obtaining a copy of these files from the author, directly or
 * indirectly, is granted, free of charge, a full and unrestricted irrevocable,
 * world-wide, paid up, royalty-free, nonexclusive right and license to deal
 * in this software and documentation files (the "Software"), including without
 * limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons who receive
 * copies from any such party to do so, with the only requirement being
 * that this copyright notice remain intact.
 */

// Ported to Java 12/00 K Weiner

#endregion

using System;

namespace MSIT.NGif
{
    public class NeuQuant
    {
        private const int NetSize = 256; /* number of colours used */
        /* four primes near 500 - assume no image has a length so large */
        /* that it is divisible by all four primes */
        private const int Prime1 = 499;
        private const int Prime2 = 491;
        private const int Prime3 = 487;
        private const int Prime4 = 503;
        private const int Minpicturebytes = (3*Prime4);
        /* minimum size for input image */
        /* Program Skeleton
		   ----------------
		   [select samplefac in range 1..30]
		   [read image from input file]
		   pic = (unsigned char*) malloc(3*width*height);
		   initnet(pic,3*width*height,samplefac);
		   learn();
		   unbiasnet();
		   [write output image header, using writecolourmap(f)]
		   inxbuild();
		   write output image using inxsearch(b,g,r)      */

        /* Network Definitions
		   ------------------- */
        private const int MaxNetPos = (NetSize - 1);
        private const int NetBiasShift = 4; /* bias for colour values */
        private const int NumCycles = 100; /* no. of learning cycles */

        /* defs for freq and bias */
        private const int BiasShift = 16; /* bias for fractions */
        private const int Bias = ((1) << BiasShift);
        private const int GammaShift = 10; /* gamma = 1024 */
        private const int BetaShift = 10;
        private const int Beta = (Bias >> BetaShift); /* beta = 1/1024 */
        private const int BetaGamma = (Bias << (GammaShift - BetaShift));

        /* defs for decreasing radius factor */
        private const int InitRad = (NetSize >> 3); /* for 256 cols, radius starts */
        private const int RadiusBiasShift = 6; /* at 32.0 biased by 6 bits */
        private const int RadiusBias = ((1) << RadiusBiasShift);
        private const int InitRadius = (InitRad*RadiusBias); /* and decreases by a */
        private const int RadiusDec = 30; /* factor of 1/30 each cycle */

        /* defs for decreasing alpha factor */
        private const int AlphaBiasShift = 10; /* alpha starts at 1.0 */
        private const int InitAlpha = ((1) << AlphaBiasShift);

        /* radbias and alpharadbias used for radpower calculation */
        private const int RadBiasShift = 8;
        private const int RadBias = ((1) << RadBiasShift);
        private const int AlphaRadBShift = (AlphaBiasShift + RadBiasShift);
        private const int AlphaRadBias = ((1) << AlphaRadBShift);
        private int _alphadec; /* biased by 10 bits */

        /* Types and Global Variables
		-------------------------- */

        /* for network lookup - really 256 */

        private readonly int[] _bias = new int[NetSize];
        /* bias and freq arrays for learning */
        private readonly int[] _freq = new int[NetSize];
        private readonly int _lengthcount; /* lengthcount = H*W*3 */
        private readonly int[] _netindex = new int[256];
        private readonly int[][] _network; /* the network itself - [netsize][4] */
        private readonly int[] _radpower = new int[InitRad];
        private int _samplefac; /* sampling factor 1..30 */
        private readonly byte[] _thepicture; /* the input image itself */
        /* radpower for precomputation */

        /* Initialise network in range (0,0,0) to (255,255,255) and set parameters
		   ----------------------------------------------------------------------- */

        public NeuQuant(byte[] thepic, int len, int sample = 10)
        {
            int i;

            _thepicture = thepic;
            _lengthcount = len;
            _samplefac = sample;

            _network = new int[NetSize][];
            for (i = 0; i < NetSize; i++) {
                _network[i] = new int[4];
                int[] p = _network[i];
                p[0] = p[1] = p[2] = (i << (NetBiasShift + 8))/NetSize;
                _freq[i] = Bias/NetSize; /* 1/netsize */
                _bias[i] = 0;
            }
        }

        private byte[] ColorMap()
        {
            byte[] map = new byte[3*NetSize];
            int[] index = new int[NetSize];
            for (int i = 0; i < NetSize; i++) index[_network[i][3]] = i;
            int k = 0;
            for (int i = 0; i < NetSize; i++) {
                int j = index[i];
                map[k++] = (byte)(_network[j][0]);
                map[k++] = (byte)(_network[j][1]);
                map[k++] = (byte)(_network[j][2]);
            }
            return map;
        }

        /* Insertion sort of network and building of netindex[0..255] (to do after unbias)
		   ------------------------------------------------------------------------------- */

        private void Inxbuild()
        {
            int previouscol = 0, startpos = 0;

            for (int i = 0; i < NetSize; i++) {
                int[] p = _network[i];
                int smallpos = i;
                int smallval = p[1];
                /* find smallest in i..netsize-1 */
                int[] q;
                for (int j = i + 1; j < NetSize; j++) {
                    q = _network[j];
                    if (q[1] >= smallval) continue;
                    /* index on g */
                    smallpos = j;
                    smallval = q[1]; /* index on g */
                }
                q = _network[smallpos];
                /* swap p (i) and q (smallpos) entries */
                if (i != smallpos) {
                    Swap(ref q[0], ref p[0]);
                    Swap(ref q[1], ref p[1]);
                    Swap(ref q[2], ref p[2]);
                    Swap(ref q[3], ref p[3]);
                }
                /* smallval entry is now in position i */
                if (smallval == previouscol) continue;
                _netindex[previouscol] = (startpos + i) >> 1;
                for (int j = previouscol + 1; j < smallval; j++) _netindex[j] = i;
                previouscol = smallval;
                startpos = i;
            }
            _netindex[previouscol] = (startpos + MaxNetPos) >> 1;
            for (int j = previouscol + 1; j < 256; j++) _netindex[j] = MaxNetPos; /* really 256 */
        }

        private static void Swap<T>(ref T one, ref T two)
        {
            T r = one;
            one = two;
            two = r;
        }

        /* Main Learning Loop
		   ------------------ */

        private void Learn()
        {
            int i;
            int step;

            if (_lengthcount < Minpicturebytes) _samplefac = 1;
            _alphadec = 30 + ((_samplefac - 1)/3);
            byte[] p = _thepicture;
            int pix = 0;
            int lim = _lengthcount;
            int samplepixels = _lengthcount/(3*_samplefac);
            int delta = samplepixels/NumCycles;
            int alpha = InitAlpha;
            int radius = InitRadius;

            int rad = radius >> RadiusBiasShift;
            if (rad <= 1) rad = 0;
            for (i = 0; i < rad; i++) _radpower[i] = alpha*(((rad*rad - i*i)*RadBias)/(rad*rad));

            if (_lengthcount < Minpicturebytes) step = 3;
            else if ((_lengthcount%Prime1) != 0) step = 3*Prime1;
            else if ((_lengthcount%Prime2) != 0) step = 3*Prime2;
            else if ((_lengthcount%Prime3) != 0) step = 3*Prime3;
            else step = 3*Prime4;

            i = 0;
            while (i < samplepixels) {
                int b = (p[pix + 0] & 0xff) << NetBiasShift;
                int g = (p[pix + 1] & 0xff) << NetBiasShift;
                int r = (p[pix + 2] & 0xff) << NetBiasShift;
                int j = Contest(b, g, r);

                Altersingle(alpha, j, b, g, r);
                if (rad != 0) Alterneigh(rad, j, b, g, r); /* alter neighbours */

                pix += step;
                if (pix >= lim) pix -= _lengthcount;

                i++;
                if (delta == 0) delta = 1;
                if (i%delta != 0) continue;
                alpha -= alpha/_alphadec;
                radius -= radius/RadiusDec;
                rad = radius >> RadiusBiasShift;
                if (rad <= 1) rad = 0;
                for (j = 0; j < rad; j++) _radpower[j] = alpha*(((rad*rad - j*j)*RadBias)/(rad*rad));
            }
            //fprintf(stderr,"finished 1D learning: readonly alpha=%f !\n",((float)alpha)/initalpha);
        }

        /* Search for BGR values 0..255 (after net is unbiased) and return colour index
		   ---------------------------------------------------------------------------- */

        public int Map(int b, int g, int r)
        {
            int bestd = 1000;
            int best = -1;
            int i = _netindex[g];
            int j = i - 1;

            while ((i < NetSize) || (j >= 0)) {
                int dist;
                int a;
                int[] p;
                if (i < NetSize) {
                    p = _network[i];
                    dist = p[1] - g; /* inx key */
                    if (dist >= bestd) i = NetSize; /* stop iter */
                    else {
                        i++;
                        if (dist < 0) dist = -dist;
                        a = p[0] - b;
                        if (a < 0) a = -a;
                        dist += a;
                        if (dist < bestd) {
                            a = p[2] - r;
                            if (a < 0) a = -a;
                            dist += a;
                            if (dist < bestd) {
                                bestd = dist;
                                best = p[3];
                            }
                        }
                    }
                }
                if (j < 0) continue;
                p = _network[j];
                dist = g - p[1]; /* inx key - reverse dif */
                if (dist >= bestd) j = -1; /* stop iter */
                else {
                    j--;
                    dist = Math.Abs(dist);
                    a = Math.Abs(p[0] - b);
                    dist += a;
                    if (dist >= bestd) continue;
                    a = Math.Abs(p[2] - r);
                    dist += a;
                    if (dist >= bestd) continue;
                    bestd = dist;
                    best = p[3];
                }
            }
            return best;
        }

        public byte[] Process()
        {
            Learn();
            Unbiasnet();
            Inxbuild();
            return ColorMap();
        }

        /* Unbias network to give byte values 0..255 and record position i to prepare for sort
		   ----------------------------------------------------------------------------------- */

        private void Unbiasnet()
        {
            for (int i = 0; i < NetSize; i++) {
                _network[i][0] >>= NetBiasShift;
                _network[i][1] >>= NetBiasShift;
                _network[i][2] >>= NetBiasShift;
                _network[i][3] = i; /* record colour no */
            }
        }

        /* Move adjacent neurons by precomputed alpha*(1-((i-j)^2/[r]^2)) in radpower[|i-j|]
		   --------------------------------------------------------------------------------- */

        private void Alterneigh(int rad, int i, int b, int g, int r)
        {
            int lo = i - rad;
            if (lo < -1) lo = -1;
            int hi = i + rad;
            if (hi > NetSize) hi = NetSize;

            int j = i + 1;
            int k = i - 1;
            int m = 1;
            while ((j < hi) || (k > lo)) {
                int a = _radpower[m++];
                int[] p;
                if (j < hi) {
                    p = _network[j++];
                    try {
                        p[0] -= (a*(p[0] - b))/AlphaRadBias;
                        p[1] -= (a*(p[1] - g))/AlphaRadBias;
                        p[2] -= (a*(p[2] - r))/AlphaRadBias;
                    } catch {} // prevents 1.3 miscompilation
                }
                if (k <= lo) continue;
                p = _network[k--];
                try {
                    p[0] -= (a*(p[0] - b))/AlphaRadBias;
                    p[1] -= (a*(p[1] - g))/AlphaRadBias;
                    p[2] -= (a*(p[2] - r))/AlphaRadBias;
                } catch {}
            }
        }

        /* Move neuron i towards biased (b,g,r) by factor alpha
		   ---------------------------------------------------- */

        private void Altersingle(int alpha, int i, int b, int g, int r)
        {
            /* alter hit neuron */
            int[] n = _network[i];
            n[0] -= (alpha*(n[0] - b))/InitAlpha;
            n[1] -= (alpha*(n[1] - g))/InitAlpha;
            n[2] -= (alpha*(n[2] - r))/InitAlpha;
        }

        /* Search for biased BGR values
		   ---------------------------- */

        private int Contest(int b, int g, int r)
        {
            /* finds closest neuron (min dist) and updates freq */
            /* finds best neuron (min dist-bias) and returns position */
            /* for frequently chosen neurons, freq[i] is high and bias[i] is negative */
            /* bias[i] = gamma*((1/netsize)-freq[i]) */

            int bestd = ~((1) << 31);
            int bestbiasd = bestd;
            int bestpos = -1;
            int bestbiaspos = bestpos;

            for (int i = 0; i < NetSize; i++) {
                int[] n = _network[i];
                int dist = n[0] - b;
                if (dist < 0) dist = -dist;
                int a = n[1] - g;
                if (a < 0) a = -a;
                dist += a;
                a = n[2] - r;
                if (a < 0) a = -a;
                dist += a;
                if (dist < bestd) {
                    bestd = dist;
                    bestpos = i;
                }
                int biasdist = dist - ((_bias[i]) >> (BiasShift - NetBiasShift));
                if (biasdist < bestbiasd) {
                    bestbiasd = biasdist;
                    bestbiaspos = i;
                }
                int betafreq = (_freq[i] >> BetaShift);
                _freq[i] -= betafreq;
                _bias[i] += (betafreq << GammaShift);
            }
            _freq[bestpos] += Beta;
            _bias[bestpos] -= BetaGamma;
            return (bestbiaspos);
        }
    }
}