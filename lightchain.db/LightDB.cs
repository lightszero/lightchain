using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


namespace lightchain.db
{


    public class LightDB
    {
        public Version Version => typeof(LightDB).Assembly.GetName().Version;


        RocksDbSharp.RocksDb db;
        public void Open(string path)
        {
            RocksDbSharp.DbOptions option = new RocksDbSharp.DbOptions();
            option.SetCreateIfMissing(true);
            option.SetCompression(RocksDbSharp.CompressionTypeEnum.rocksdb_snappy_compression);
            this.db = RocksDbSharp.RocksDb.Open(option, path);
        }
        public void Close()
        {
            this.db.Dispose();
            this.db = null;
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
        public WriteBatch CreateWriteBatch(SnapShot snapshot)
        {
            return new WriteBatch(this.db, snapshot);
        }


        //往数据库里写入一块数据
        public void Write(WriteBatch batch)
        {
            db.Write(batch.batch);
        }


    }
}
