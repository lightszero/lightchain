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

            snapshotLast = CreateSnapInfo();
            snapshotLast.refCount++;
        }
        public void Close()
        {
            this.db.Dispose();
            this.db = null;
        }
        private SnapShot snapshotLast;


        //如果 height=0，取最新的快照
        public ISnapShot UseSnapShot()
        {
            snapshotLast.refCount++;
            return snapshotLast;
        }
        //创建快照
        private SnapShot CreateSnapInfo()
        {
            //看最新高度的快照是否已经产生
            var snapshot = new SnapShot(this.db);
            snapshot.readop = new RocksDbSharp.ReadOptions();
            snapshot.snapshot = db.CreateSnapshot();
            snapshot.readop.SetSnapshot(snapshot.snapshot);
            return snapshot;
        }
        public WriteTask CreateWriteTask()
        {
            return new WriteTask();
        }
        static readonly byte[] systemtable_block = new byte[] { 0x01 };
        static readonly byte[] systemtable_info = new byte[] { 0x00 };
        public void Write(WriteTask task)
        {
            using (var wb = new WriteBatch(this.db, snapshotLast))
            {
                //var taskblock = task.Tobytes(height);
                //还要把这个block本身写入，高度写入
                //wb.Put(systemtable_block, height, taskblock);
                foreach (var item in task.items)
                {
                    switch (item.op)
                    {
                        case WriteTaskOP.CreateTable:
                            if (item.tableID.Length < 2)
                                throw new Exception("not allow too short table id.");
                            wb.CreateTable(item.tableID, item.value);
                            break;
                        case WriteTaskOP.DeleteTable:
                            if (item.tableID.Length < 2)
                                throw new Exception("not allow too short table id.");
                            wb.DeleteTable(item.tableID);
                            break;
                        case WriteTaskOP.PutValue:
                            if (item.tableID.Length < 2)
                                throw new Exception("not allow too short table id.");
                            wb.PutUnsafe(item.tableID, item.key, item.value);
                            break;
                        case WriteTaskOP.DeleteValue:
                            if (item.tableID.Length < 2)
                                throw new Exception("not allow too short table id.");
                            wb.Delete(item.tableID, item.key);
                            break;
                        case WriteTaskOP.Log:
                            break;
                    }
                }
                this.db.Write(wb.batch);
                snapshotLast.Dispose();
                snapshotLast = CreateSnapInfo();
                snapshotLast.refCount++;
            }
        }
        //往数据库里写入一块数据
        //public void Write(WriteBatch batch)
        //{
        //    db.Write(batch.batch);
        //}


    }
}
