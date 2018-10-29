using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


namespace lightchain.db
{


    public class SnapShotInfo
    {
        public ulong height;
        public uint useCount;
        public RocksDbSharp.ReadOptions readop;
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
        static byte[] HeadTableList = new byte[1] { 0x01 };
        static byte[] HeadNestedTableList = new byte[1] { 0x02 };

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

        //往数据库里写入一块数据
        public void WriteBlock(WriteBlock block)
        {
            RocksDbSharp.WriteBatch wb = new RocksDbSharp.WriteBatch();

        }
        public object GetTableInfo(SnapShotInfo snapshot, byte[] key)
        {
            return null;
        }
        public RocksDbSharp.Iterator Find(SnapShotInfo snapshot, byte[] table, byte[] keybegin = null, byte[] keyend = null)
        {
            //db.NewIterator();
            return null;
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
