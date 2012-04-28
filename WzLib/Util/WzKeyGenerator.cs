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

using System;
using System.IO;
using System.Security.Cryptography;

namespace MSIT.WzLib.Util
{
    public class WzKeyGenerator
    {
        #region Methods

        /// <summary>
        ///   Generates the wz key used in the encryption from ZLZ.dll
        /// </summary>
        /// <param name="pathToZlz"> Path to ZLZ.dll </param>
        /// <returns> The wz key </returns>
        public static byte[] GenerateKeyFromZlz(string pathToZlz)
        {
            FileStream zlzStream = File.OpenRead(pathToZlz);
            byte[] wzKey = GenerateWzKey(GetIvFromZlz(zlzStream), GetAesKeyFromZlz(zlzStream));
            zlzStream.Close();
            return wzKey;
        }

        public static byte[] GetIvFromZlz(FileStream zlzStream)
        {
            byte[] iv = new byte[4];

            zlzStream.Seek(0x10040, SeekOrigin.Begin);
            zlzStream.Read(iv, 0, 4);
            return iv;
        }

        private static byte[] GetAesKeyFromZlz(FileStream zlzStream)
        {
            byte[] aes = new byte[32];

            zlzStream.Seek(0x10060, SeekOrigin.Begin);
            for (int i = 0; i < 8; i++)
            {
                zlzStream.Read(aes, i*4, 4);
                zlzStream.Seek(12, SeekOrigin.Current);
            }
            return aes;
        }

        public static byte[] GenerateWzKey(byte[] wzIv)
        {
            return GenerateWzKey(wzIv, CryptoConstants.UserKey);
        }

        private static byte[] GenerateWzKey(byte[] wzIv, byte[] aesKey)
        {
            AesManaged crypto = new AesManaged();
            crypto.KeySize = 256;
            crypto.Key = aesKey;
            crypto.Mode = CipherMode.ECB;

            MemoryStream memStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memStream, crypto.CreateEncryptor(), CryptoStreamMode.Write);
            try
            {
                byte[] input = MultiplyBytes(wzIv, 4, 4);
                byte[] wzKey = new byte[UInt16.MaxValue];
                for (int i = 0; i < (wzKey.Length/16); i++)
                {
                    cryptoStream.Write(input, 0, 16);
                    input = memStream.ToArray();
                    Array.Copy(input, 0, wzKey, (i*16), 16);
                    memStream.Position = 0;
                }
                cryptoStream.Write(input, 0, 16);
                Array.Copy(memStream.ToArray(), 0, wzKey, (wzKey.Length - 15), 15);
                return wzKey;
            }
            finally
            {
                cryptoStream.Dispose();
                memStream.Dispose();
            }
        }

        #endregion

        /// <summary>
        ///   Multiplies bytes
        /// </summary>
        /// <param name="input">Bytes to multiply</param>
        /// <param name="count">Amount of bytes to repeat</param>
        /// <param name="mult">Times to repeat the sequence</param>
        /// <returns> The multiplied bytes </returns>
        private static byte[] MultiplyBytes(byte[] input, int count, int mult)
        {
            byte[] ret = new byte[count*mult];
            for (int x = 0; x < ret.Length; x++)
            {
                ret[x] = input[x%count];
            }
            return ret;
        }
    }
}