using System;
using System.Collections.Generic;
using System.Text;

namespace lightchain.db
{
    public static class Helper
    {
        public static byte[] ToBytes(this string str)
        {
            return System.Text.Encoding.UTF8.GetBytes(str);
        }

        public static byte[] CalcKey(byte[] head, byte[] key)
        {
            if (head.Length > 255)
                throw new Exception("not support key >255 bytes");
            byte[] finalkey = new byte[head.Length + 1 + key.Length];
            finalkey[0] = (byte)head.Length;
            for (var i = 0; i < head.Length; i++)
            {
                finalkey[i + 1] = head[i];
            };
            for (var i = 0; i < key.Length; i++)
            {
                finalkey[i + 1 + head.Length] = key[i];
            }
            return finalkey;
        }
    }
}
