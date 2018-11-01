using System;
using System.Collections.Generic;
using System.Text;

namespace lightchain.db
{
    public enum SplitWord : byte
    {
        TableInfo = 0x00,
        TableCount = 0x01,
        TableItem = 0x02,
    }
    public static class Helper
    {
        public static byte[] ToBytes_UTF8Decode(this string str)
        {
            return System.Text.Encoding.UTF8.GetBytes(str);
        }
        public static byte[] ToBytes_HexParse(this string str)
        {
            if (str.IndexOf("0x") == 0 || str.IndexOf("0X") == 0)
            {
                str = str.Substring(2);
            }
            byte[] data = new byte[str.Length / 2];
            for (var i = 0; i < str.Length / 2; i++)
            {
                data[i] = byte.Parse(str.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
            }
            return data;
        }
        public static string ToHexString(this byte[] data)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var b in data)
            {
                sb.Append(b.ToString("x02"));
            }
            return sb.ToString();
        }

        public static byte[] CalcKey(byte[] head, byte[] key, SplitWord splitWord = SplitWord.TableItem)
        {
            if (head.Length > 255)
                throw new Exception("not support key >255 bytes");
            byte[] finalkey = new byte[1 + head.Length + 1 + (key != null ? key.Length : 0)];
            //key的构成 keylen + key + splitword + value
            //splitword 00 controlitem
            //splitword 01 valueitem
            //splitword >0 otherinfo
            finalkey[0] = (byte)head.Length;
            for (var i = 0; i < head.Length; i++)
            {
                finalkey[i + 1] = head[i];
            };
            finalkey[1 + head.Length] = (byte)splitWord;
            if (key != null)
            {
                for (var i = 0; i < key.Length; i++)
                {
                    finalkey[1 + head.Length + 1 + i] = key[i];
                }
            }
            return finalkey;
        }

    }
}
