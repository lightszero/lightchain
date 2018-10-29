using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


namespace lightchain.db
{
    static class Helper
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

    public class SnapShotInfo
    {
        public ulong height;
        public uint useCount;
        public RocksDbSharp.ReadOptions readop;
    }
    public class DBValue
    {
        public enum Type
        {
            Bytes,
            INT32,
            UINT32,
            INT64,
            UINT64,
            BOOL,
            Float32,
            Float64,
            BigNumber,
            String,
            //Struct
            //数组
            //字典 这类复杂数据结构不提供
        }
        public Type type;
        public byte[] tag;
        public ulong LastModifyHeight;//最后修改高度
        public byte[] value;
        public object typedvalue;
        private DBValue()
        {

        }
        public static DBValue FromValue(Type _type, object _value)
        {
            DBValue v = new DBValue();
            v.type = _type;
            v.tag = new byte[0];
            if (v.type == Type.Bytes && _value is byte[])
            {

            }
            else if (v.type == Type.INT32 && _value is Int32)
            {
                v.value = BitConverter.GetBytes((Int32)_value);
            }
            else if (v.type == Type.UINT32 && _value is UInt32)
            {
                v.value = BitConverter.GetBytes((UInt32)_value);

            }
            else if (v.type == Type.INT64 && _value is Int64)
            {
                v.value = BitConverter.GetBytes((Int64)_value);

            }
            else if (v.type == Type.UINT64 && _value is UInt64)
            {
                v.value = BitConverter.GetBytes((UInt64)_value);

            }
            else if (v.type == Type.BOOL && _value is bool)
            {
                v.value = ((bool)_value) ? new byte[1] { 1 } : new byte[] { 0 };
            }
            else if (v.type == Type.Float32 && _value is float)
            {
                v.value = BitConverter.GetBytes((float)_value);

            }
            else if (v.type == Type.Float64 && _value is double)
            {
                v.value = BitConverter.GetBytes((double)_value);

            }
            else if (v.type == Type.BigNumber && _value is System.Numerics.BigInteger)
            {
                v.value = ((System.Numerics.BigInteger)_value).ToByteArray();
            }
            else if (v.type == Type.String && _value is string)
            {
                v.value = System.Text.Encoding.UTF8.GetBytes((string)_value);
            }
            else
            {
                throw new Exception("error value type:want[" + _type + "] value is[" + _value.GetType() + "]");
            }
            v.typedvalue = _value;
            return v;
        }
        public static DBValue FromRaw(byte[] data)
        {
            DBValue v = new DBValue();
            //read type
            v.type = (Type)data[0];
            //read tag
            v.tag = new byte[data[1]];
            for (var i = 0; i < v.tag.Length; i++)
            {
                v.tag[i] = data[i + 2];
            }
            //read last
            v.LastModifyHeight = BitConverter.ToUInt64(data, 2 + v.tag.Length);

            //read value
            v.value = new byte[data.Length - 2 - v.tag.Length - 8];
            for (var i = 0; i < v.value.Length; i++)
            {
                v.value[i] = data[2 + v.tag.Length + 8 + i];
            }
            v.ParseValue();
            return v;
        }
        public byte[] ToBytes()
        {
            byte[] data = new byte[2 + this.tag.Length + 8 + this.value.Length];
            //write type
            data[0] = (byte)this.type;
            //write tag
            data[1] = (byte)this.tag.Length;
            for (var i = 0; i < this.tag.Length; i++)
            {
                data[i + 2] = this.tag[i];
            }
            //write last
            byte[] last = BitConverter.GetBytes(this.LastModifyHeight);
            for (var i = 0; i < 8; i++)
            {
                data[2 + this.tag.Length + i] = last[i];
            }
            //write value;
            for (var i = 0; i < this.value.Length; i++)
            {
                data[2 + this.tag.Length + 8 + i] = this.value[i];
            }
            return data;
        }

        public void ParseValue()
        {
            switch (this.type)
            {
                case Type.Bytes:
                    break;
                case Type.INT32:
                    typedvalue = BitConverter.ToInt32(this.value);
                    break;
                case Type.UINT32:
                    typedvalue = BitConverter.ToUInt32(this.value);
                    break;
                case Type.INT64:
                    typedvalue = BitConverter.ToInt64(this.value);
                    break;
                case Type.UINT64:
                    typedvalue = BitConverter.ToUInt64(this.value);
                    break;
                case Type.BOOL:
                    typedvalue = (this.value[0] > 0);
                    break;
                case Type.Float32:
                    typedvalue = BitConverter.ToSingle(this.value);
                    break;
                case Type.Float64:
                    typedvalue = BitConverter.ToDouble(this.value);
                    break;
                case Type.BigNumber:
                    typedvalue = new System.Numerics.BigInteger(this.value);
                    break;
                case Type.String:
                    typedvalue = System.Text.Encoding.UTF8.GetString(this.value);
                    break;
            }


        }
        public int AsInt32()
        {
            if (this.type == Type.INT32)
            {
                return (int)typedvalue;
            }
            else if (this.type == Type.UINT32)
            {
                return (int)(uint)typedvalue;
            }
            else if (this.type == Type.UINT64)
            {
                return (int)(ulong)typedvalue;
            }
            else if (this.type == Type.INT64)
            {
                return (int)(long)typedvalue;
            }
            else if (this.type == Type.Float32)
            {
                return (int)(float)typedvalue;
            }
            else if (this.type == Type.Float64)
            {
                return (int)(double)typedvalue;
            }
            else if (this.type == Type.BigNumber)
            {
                return (int)(System.Numerics.BigInteger)typedvalue;
            }
            else
            {
                throw new Exception("do not known how to convert it.");
            }
        }
        public uint AsUInt32()
        {
            if (this.type == Type.UINT32)
            {
                return (uint)typedvalue;
            }
            else if (this.type == Type.INT32)
            {
                return (uint)(int)typedvalue;
            }
            else if (this.type == Type.UINT64)
            {
                return (uint)(ulong)typedvalue;
            }
            else if (this.type == Type.INT64)
            {
                return (uint)(long)typedvalue;
            }
            else if (this.type == Type.Float32)
            {
                return (uint)(float)typedvalue;
            }
            else if (this.type == Type.Float64)
            {
                return (uint)(double)typedvalue;
            }
            else if (this.type == Type.BigNumber)
            {
                return (uint)(System.Numerics.BigInteger)typedvalue;
            }
            else
            {
                throw new Exception("do not known how to convert it.");
            }
        }
        public long AsInt64()
        {
            if (this.type == Type.INT64)
            {
                return (long)typedvalue;
            }
            else if (this.type == Type.UINT64)
            {
                return (long)(ulong)typedvalue;
            }
            else if (this.type == Type.UINT32)
            {
                return (long)(uint)typedvalue;
            }
            else if (this.type == Type.INT32)
            {
                return (long)(int)typedvalue;
            }
            else if (this.type == Type.Float32)
            {
                return (long)(float)typedvalue;
            }
            else if (this.type == Type.Float64)
            {
                return (long)(double)typedvalue;
            }
            else if (this.type == Type.BigNumber)
            {
                return (long)(System.Numerics.BigInteger)typedvalue;
            }
            else
            {
                throw new Exception("do not known how to convert it.");
            }
        }
        public ulong AsUInt64()
        {
            if (this.type == Type.UINT64)
            {
                return (ulong)typedvalue;
            }
            else if (this.type == Type.INT64)
            {
                return (ulong)(long)typedvalue;
            }
            else if (this.type == Type.UINT32)
            {
                return (ulong)(uint)typedvalue;
            }
            else if (this.type == Type.INT32)
            {
                return (ulong)(int)typedvalue;
            }
            else if (this.type == Type.Float32)
            {
                return (ulong)(float)typedvalue;
            }
            else if (this.type == Type.Float64)
            {
                return (ulong)(double)typedvalue;
            }
            else if (this.type == Type.BigNumber)
            {
                return (ulong)(System.Numerics.BigInteger)typedvalue;
            }
            else
            {
                throw new Exception("do not known how to convert it.");
            }
        }
        public System.Numerics.BigInteger AsBigInteger()
        {
            if (this.type == Type.BigNumber)
            {
                return (System.Numerics.BigInteger)typedvalue;
            }
            else if (this.type == Type.UINT64)
            {
                return (ulong)typedvalue;
            }
            else if (this.type == Type.INT64)
            {
                return (long)typedvalue;
            }
            else if (this.type == Type.UINT32)
            {
                return (uint)typedvalue;
            }
            else if (this.type == Type.INT32)
            {
                return (int)typedvalue;
            }
            else if (this.type == Type.Float32)
            {
                return (long)(float)typedvalue;
            }
            else if (this.type == Type.Float64)
            {
                return (long)(double)typedvalue;
            }
            else
            {
                throw new Exception("do not known how to convert it.");
            }
        }
        public float AsFloat32()
        {
            if (this.type == Type.Float32)
            {
                return (float)typedvalue;
            }
            else if (this.type == Type.Float64)
            {
                return (float)(double)typedvalue;
            }
            else if (this.type == Type.INT32)
            {
                return (float)(int)typedvalue;
            }
            else if (this.type == Type.UINT32)
            {
                return (float)(uint)typedvalue;
            }
            else if (this.type == Type.UINT64)
            {
                return (float)(ulong)typedvalue;
            }
            else if (this.type == Type.INT64)
            {
                return (float)(long)typedvalue;
            }
            else if (this.type == Type.BigNumber)
            {
                return (float)(System.Numerics.BigInteger)typedvalue;
            }
            else
            {
                throw new Exception("do not known how to convert it.");
            }
        }
        public double AsFloat64()
        {
            if (this.type == Type.Float32)
            {
                return (float)typedvalue;
            }
            else if (this.type == Type.Float64)
            {
                return (double)typedvalue;
            }
            else if (this.type == Type.INT32)
            {
                return (double)(int)typedvalue;
            }
            else if (this.type == Type.UINT32)
            {
                return (double)(uint)typedvalue;
            }
            else if (this.type == Type.UINT64)
            {
                return (double)(ulong)typedvalue;
            }
            else if (this.type == Type.INT64)
            {
                return (double)(long)typedvalue;
            }
            else if (this.type == Type.BigNumber)
            {
                return (double)(System.Numerics.BigInteger)typedvalue;
            }
            else
            {
                throw new Exception("do not known how to convert it.");
            }
        }
        public string AsString()
        {
            if (this.type == Type.String)
            {
                return (string)typedvalue;
            }
            else
            {
                throw new Exception("do not known how to convert it.");
            }
        }
        public bool AsBool()
        {
            if (this.type == Type.BOOL)
            {
                return (bool)typedvalue;
            }
            else
            {
                throw new Exception("do not known how to convert it.");
            }
        }
    }

    public class LightChainDB
    {
        public Version Version => typeof(LightChainDB).Assembly.GetName().Version;
        public uint CheckPointStep
        {
            get;
            private set;
        }

        RocksDbSharp.RocksDb db;
        public void Init(string path, uint checkpointStep = 1000)
        {
            RocksDbSharp.DbOptions option = new RocksDbSharp.DbOptions();
            option.SetCreateIfMissing(true);
            option.SetCompression(RocksDbSharp.CompressionTypeEnum.rocksdb_snappy_compression);
            this.db = RocksDbSharp.RocksDb.Open(option, path);
            this.CheckPointStep = checkpointStep;

            //先做数据库的初始化
            var height = this.GetSystemValue(null, "_height".ToBytes());
            if (height == null)
            {
                //这是个空的数据库，初始化一下
                SnapShotInfo snapshot = CreateSnapInfo();
                RocksDbSharp.WriteBatch wb = new RocksDbSharp.WriteBatch();
                DBValue v = DBValue.FromValue(DBValue.Type.UINT64, (ulong)0);
                wb.Put(Helper.CalcKey(HeadSystem, "_height".ToBytes()), v.ToBytes());

                db.Write(wb);
            }
        }
        static byte[] HeadSystem = new byte[1] { 0x00 };


        System.Collections.Concurrent.ConcurrentDictionary<ulong, SnapShotInfo> allSnapshot = new System.Collections.Concurrent.ConcurrentDictionary<ulong, SnapShotInfo>();

        public ulong Height
        {
            get;
            private set;
        }
        //获取最新快照
        public SnapShotInfo CreateSnapInfo()
        {
            //看最新高度的快照是否已经产生
            var b = allSnapshot.TryGetValue(this.Height, out SnapShotInfo value);
            if (b)
            {
                return value;
            }
            else
            {
                var snapshot = new SnapShotInfo();
                snapshot.readop = new RocksDbSharp.ReadOptions();
                snapshot.readop.SetSnapshot(db.CreateSnapshot());
                var heightvalue = this.GetSystemValue(snapshot, "_height".ToBytes());
                var height = heightvalue == null ? 0 : heightvalue.AsUInt64();
                snapshot.height = height;
                snapshot.useCount = 1;
                return snapshot;
            }
        }
        //得到指定快照
        public SnapShotInfo GetSnapShot(ulong height)
        {
            //指定高度，要么返回要么没有
            var b = allSnapshot.TryGetValue(height, out SnapShotInfo value);
            if (b)
            {
                value.useCount++;
                return value;
            }
            else
            {
                return null;
            }
        }
        //关闭快照，用完
        public void CloseSnapShot(SnapShotInfo snapshot)
        {
            snapshot.useCount--;
        }


        public DBValue GetSystemValue(SnapShotInfo snapshot, byte[] key)
        {

            byte[] finialkey = Helper.CalcKey(HeadSystem, key);
            var value = this.db.Get(finialkey, null, snapshot?.readop);
            return value == null ? null : DBValue.FromRaw(value);
        }
        public byte[] GetValue(SnapShotInfo snapshot, byte[] table, byte[] key)
        {
            byte[] finialkey = Helper.CalcKey(table, key);
            return this.db.Get(finialkey, null, snapshot?.readop);
        }

    }
}
