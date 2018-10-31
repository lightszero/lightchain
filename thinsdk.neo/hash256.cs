using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
namespace ThinNeo
{
    public class Hash256 : IComparable<Hash256>
    {
        public Hash256(byte[] data)
        {
            if (data.Length != 32)
                throw new Exception("error length.");
            this.data = data;
        }
        public Hash256(string hexstr)
        {
            var bts = Helper.HexString2Bytes(hexstr);
            if (bts.Length != 32)
                throw new Exception("error length.");
            this.data = bts.Reverse().ToArray();
        }
        public override string ToString()
        {
            return "0x" + Helper.Bytes2HexString(this.data.Reverse().ToArray());
        }
        public byte[] data;

        public int CompareTo(Hash256 other)
        {
            byte[] x = data;
            byte[] y = other.data;
            for (int i = x.Length - 1; i >= 0; i--)
            {
                if (x[i] > y[i])
                    return 1;
                if (x[i] < y[i])
                    return -1;
            }
            return 0;
        }
        public override bool Equals(object obj)
        {
            return CompareTo(obj as Hash256) == 0;
        }
        public override int GetHashCode()
        {
            return new System.Numerics.BigInteger(data).GetHashCode();
        }
        public static implicit operator byte[] (Hash256 value)
        {
            return value.data;
        }
        public static implicit operator Hash256(byte[] value)
        {
            return new Hash256(value);
        }
    }
}
