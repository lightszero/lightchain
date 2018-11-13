using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThinNeo
{
    public static class Helper
    {
        [ThreadStatic]
        static System.Security.Cryptography.SHA256 _sha256;
        public static System.Security.Cryptography.SHA256 Sha256
        {
            get
            {
                if (_sha256 == null)
                    _sha256 = System.Security.Cryptography.SHA256.Create();
                return _sha256;
            }
        }
        [ThreadStatic]
        static ThinNeo.Cryptography.RIPEMD160Managed _ripemd160;
        public static ThinNeo.Cryptography.RIPEMD160Managed RIPEMD160
        {
            get
            {
                if (_ripemd160 == null)
                    _ripemd160 = new ThinNeo.Cryptography.RIPEMD160Managed();
                return _ripemd160;
            }
        }
        [ThreadStatic]
        static System.Security.Cryptography.RandomNumberGenerator _random;
        public static System.Security.Cryptography.RandomNumberGenerator Random
        {
            get
            {
                if (_random == null)
                    _random = System.Security.Cryptography.RandomNumberGenerator.Create();
                return _random;
            }
        }
        [ThreadStatic]
        static System.Random _randomquick;
        public static System.Random RandomQuick
        {
            get
            {
                if (_randomquick == null)
                    _randomquick = new System.Random();
                return _randomquick;
            }
        }

        public static byte[] RandomBytes(int size)
        {
            byte[] data = new byte[size];
            Random.GetBytes(data, 0, data.Length);
            return data;
        }
        public static string Bytes2HexString(byte[] data)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var d in data)
            {
                sb.Append(d.ToString("x02"));
            }
            return sb.ToString();
        }
        public static byte[] HexString2Bytes(string str)
        {
            if (str.IndexOf("0x") == 0)
                str = str.Substring(2);
            byte[] outd = new byte[str.Length / 2];
            for (var i = 0; i < str.Length / 2; i++)
            {
                outd[i] = byte.Parse(str.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
            }
            return outd;
        }

        public static byte[] CalcSha256(byte[] data, int start = 0, int length = -1)
        {
            byte[] tdata = null;

            if (start == 0 && length == -1)
            {
                tdata = data;
            }
            else
            {
                tdata = new byte[length];
                Array.Copy(data, 0, tdata, 0, length);
            }
            System.Security.Cryptography.SHA256 sha256 = System.Security.Cryptography.SHA256.Create();
            return sha256.ComputeHash(tdata);

        }

        public static byte[] Base58CheckDecode(string input)
        {
            byte[] buffer = ThinNeo.Cryptography.Base58.Decode(input);
            if (buffer.Length < 4) throw new FormatException();

            var b1 = CalcSha256(buffer, 0, buffer.Length - 4);

            byte[] checksum = CalcSha256(b1);

            if (!buffer.Skip(buffer.Length - 4).SequenceEqual(checksum.Take(4)))
                throw new FormatException();
            return buffer.Take(buffer.Length - 4).ToArray();
        }
        public static string Base58CheckEncode(byte[] data)
        {
            var b1 = CalcSha256(data);
            byte[] checksum = CalcSha256(b1);
            byte[] buffer = new byte[data.Length + 4];
            Buffer.BlockCopy(data, 0, buffer, 0, data.Length);
            Buffer.BlockCopy(checksum, 0, buffer, data.Length, 4);
            return ThinNeo.Cryptography.Base58.Encode(buffer);
        }
        internal static byte[] AES256Encrypt(byte[] block, byte[] key)
        {
            using (System.Security.Cryptography.Aes aes = System.Security.Cryptography.Aes.Create())
            {
                aes.Key = key;
                aes.Mode = System.Security.Cryptography.CipherMode.ECB;
                aes.Padding = System.Security.Cryptography.PaddingMode.None;
                using (System.Security.Cryptography.ICryptoTransform encryptor = aes.CreateEncryptor())
                {
                    return encryptor.TransformFinalBlock(block, 0, block.Length);
                }
            }
        }
        internal static byte[] AES256Decrypt(byte[] block, byte[] key)
        {
            using (System.Security.Cryptography.Aes aes = System.Security.Cryptography.Aes.Create())
            {
                aes.Key = key;
                aes.Mode = System.Security.Cryptography.CipherMode.ECB;
                aes.Padding = System.Security.Cryptography.PaddingMode.None;
                using (System.Security.Cryptography.ICryptoTransform decryptor = aes.CreateDecryptor())
                {
                    return decryptor.TransformFinalBlock(block, 0, block.Length);
                }
            }
        }
        public static byte[] XOR(byte[] x, byte[] y)
        {
            if (x.Length != y.Length) throw new ArgumentException();
            return x.Zip(y, (a, b) => (byte)(a ^ b)).ToArray();
        }
    }
}
