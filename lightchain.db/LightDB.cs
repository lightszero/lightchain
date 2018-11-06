using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


namespace lightchain.db
{

    public class DBCreateOption
    {
        public string MagicStr;//设定一个魔法字符串，作为数据库的创建字符串
        public WriteTask FirstTask;//初始化数据库时要同时完成的任务
    }
    public class LightDB
    {
        public Version Version => typeof(LightDB).Assembly.GetName().Version;


        RocksDbSharp.RocksDb db;
        IntPtr defaultWriteOpPtr;
        IntPtr defaultReadOpPtr;
        public void Open(string path, DBCreateOption createOption = null)
        {
            this.defaultWriteOpPtr = RocksDbSharp.Native.Instance.rocksdb_writeoptions_create();
            RocksDbSharp.DbOptions option = new RocksDbSharp.DbOptions();
            option.SetCreateIfMissing(true);
            option.SetCompression(RocksDbSharp.CompressionTypeEnum.rocksdb_snappy_compression);
            this.db = RocksDbSharp.RocksDb.Open(option, path);

            snapshotLast = CreateSnapInfo();
            if (snapshotLast.DataHeight == 0)
            {
                InitFirstBlock(createOption);
            }
            snapshotLast.AddRef();
        }
        public void CheckPoint(string path)
        {
            IntPtr cp =
           RocksDbSharp.Native.Instance.rocksdb_checkpoint_object_create(db.Handle);

            RocksDbSharp.Native.Instance.rocksdb_checkpoint_create(cp, path, 1024 * 1024 * 4);

            RocksDbSharp.Native.Instance.rocksdb_checkpoint_object_destroy(cp);
        }

        private void InitFirstBlock(DBCreateOption createOption)
        {
            //数据库需要初始化
            if (createOption == null)
            {
                this.Close();
                throw new Exception("数据库需要初始化 open(path,createOption)");
            }
            else
            {
                var writetask = new WriteTask();
                writetask.CreateTable(new TableInfo(systemtable_info, "_table_info", null, DBValue.Type.String));
                writetask.CreateTable(new TableInfo(systemtable_block, "_table_block", null, DBValue.Type.UINT64));

                if (createOption.FirstTask != null)
                {
                    foreach (var t in createOption.FirstTask.items)
                    {
                        writetask.items.Add(t);
                    }
                }
                this.WriteUnsafe(writetask);
            }
        }
        private void AddHeight(WriteTask task)
        {

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
            var snap = snapshotLast;

            snap.AddRef();
            return snap;
        }
        //创建快照
        private SnapShot CreateSnapInfo()
        {
            //看最新高度的快照是否已经产生
            var snapshot = new SnapShot(this.db);
            snapshot.Init();
            return snapshot;
        }
        public WriteTask CreateWriteTask()
        {
            return new WriteTask();
        }
        public static readonly byte[] systemtable_block = new byte[] { 0x01 };
        public static readonly byte[] systemtable_info = new byte[] { 0x00 };

        //写入操作需要保持线性，线程安全
        private void WriteUnsafe(WriteTask task)
        {
            lock (systemtable_block)
            {
                using (var wb = new WriteBatch(this.db.Handle, snapshotLast))
                {
                    var heightbuf = BitConverter.GetBytes(snapshotLast.DataHeight);
                    foreach (var item in task.items)
                    {
                        if (item.value != null)
                        {
                            DBValue.QuickFixHeight(item.value, heightbuf);
                        }
                        switch (item.op)
                        {
                            case WriteTaskOP.CreateTable:
                                wb.CreateTable(item.tableID, item.value);
                                break;
                            case WriteTaskOP.DeleteTable:
                                wb.DeleteTable(item.tableID);
                                break;
                            case WriteTaskOP.PutValue:
                                wb.PutUnsafe(item.tableID, item.key, item.value);
                                break;
                            case WriteTaskOP.DeleteValue:
                                wb.Delete(item.tableID, item.key);
                                break;
                            case WriteTaskOP.Log:
                                break;
                        }
                    }
                    var taskblock = task.ToBytes();
                    //还要把这个block本身写入，高度写入
                    var finaldata = DBValue.FromValue(DBValue.Type.Bytes, taskblock).ToBytes();
                    DBValue.QuickFixHeight(finaldata, heightbuf);
                    var blockkey = BitConverter.GetBytes(snapshotLast.DataHeight);
                    wb.PutUnsafe(systemtable_block, blockkey, finaldata);
                    //wb.Put(systemtable_block, height, taskblock);

                    //height++
                    var finalheight = DBValue.FromValue(DBValue.Type.UINT64, (ulong)(snapshotLast.DataHeight + 1)).ToBytes();
                    DBValue.QuickFixHeight(finalheight, heightbuf);
                    wb.PutUnsafe(systemtable_info, "_height".ToBytes_UTF8Encode(), finalheight);

                    RocksDbSharp.Native.Instance.rocksdb_write(this.db.Handle, this.defaultWriteOpPtr, wb.batchptr);
                    //this.db.Write(wb.batch);
                    snapshotLast.Dispose();
                    snapshotLast = CreateSnapInfo();
                    snapshotLast.AddRef();
                }
            }
        }
        public void Write(WriteTask task)
        {
            foreach (var item in task.items)
            {
                if (item.tableID != null && item.tableID.Length < 2)
                    throw new Exception("table id is too short.");
            }
            WriteUnsafe(task);
        }
        //往数据库里写入一块数据
        //public void Write(WriteBatch batch)
        //{
        //    db.Write(batch.batch);
        //}


    }
}
