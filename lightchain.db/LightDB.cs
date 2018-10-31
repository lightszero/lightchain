using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


namespace lightchain.db
{
  
  
    public class SnapShot : IDisposable
    {
        public SnapShot(RocksDbSharp.RocksDb db)
        {
            this.db = db;
        }
        public RocksDbSharp.RocksDb db;
        public RocksDbSharp.ReadOptions readop;
        public RocksDbSharp.Snapshot snapshot;
        public void Dispose()
        {
            snapshot.Dispose();
        }
        public byte[] GetValueData(byte[] tablehead, byte[] key)
        {
            byte[] finialkey = Helper.CalcKey(tablehead, key);
            return this.db.Get(finialkey, null, readop);
        }
        public DBValue GetValue(byte[] tablehead, byte[] key)
        {
            return DBValue.FromRaw(GetValueData(tablehead, key));
        }
        public TableKeyFinder CreateKeyFinder(byte[] tablehead, byte[] beginkey = null, byte[] endkey = null)
        {
            TableKeyFinder find = new TableKeyFinder(this, tablehead, beginkey, endkey);
            return find;
        }
        public TableIterator CreateKeyIterator(byte[] tablehead, byte[] _beginkey = null, byte[] _endkey = null)
        {
            var beginkey = Helper.CalcKey(tablehead, _beginkey);
            var endkey = Helper.CalcKey(tablehead, _endkey);
            return new TableIterator(this, beginkey, endkey);
        }
        public TableInfo GetTableInfo(byte[] tablehead)
        {
            var tablekey = Helper.CalcKey(tablehead, null, SplitWord.TableInfo);
            var data = this.db.Get(tablekey, null, readop);
            return TableInfo.FromRaw(data);
        }
        public uint GetTableCount(byte[] tablehead)
        {
            var tablekey = Helper.CalcKey(tablehead, null, SplitWord.TableCount);
            var data = this.db.Get(tablekey, null, readop);
            return DBValue.FromRaw(data).AsUInt32();
        }
    }
    public class WriteBatch
    {
        public WriteBatch(RocksDbSharp.RocksDb db)
        {
            this.db = db;
        }
        RocksDbSharp.RocksDb db;
        public RocksDbSharp.WriteBatch batch;
        public void Dispose()
        {
            if (batch != null)
            {
                batch.Dispose();
                batch = null;
            }
        }
    }



    public class LightDB
    {
        public Version Version => typeof(LightDB).Assembly.GetName().Version;


        RocksDbSharp.RocksDb db;
        public void Init(string path)
        {
            RocksDbSharp.DbOptions option = new RocksDbSharp.DbOptions();
            option.SetCreateIfMissing(true);
            option.SetCompression(RocksDbSharp.CompressionTypeEnum.rocksdb_snappy_compression);
            this.db = RocksDbSharp.RocksDb.Open(option, path);
        }

        //创建快照
        public SnapShot CreateSnapInfo()
        {
            //看最新高度的快照是否已经产生
            var snapshot = new SnapShot(this.db);
            snapshot.readop = new RocksDbSharp.ReadOptions();
            snapshot.snapshot = db.CreateSnapshot();
            snapshot.readop.SetSnapshot(snapshot.snapshot);
            return snapshot;
        }


        //往数据库里写入一块数据
        public void Write(WriteBatch batch)
        {
            db.Write(batch.batch);
        }


    }
}
