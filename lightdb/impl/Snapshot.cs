using RocksDbSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace LightDB
{

    /// <summary>
    /// snapshot 快照，是rocksdb的重要功能，用快照来读取，无论此时数据如何被写入，都不会影响读取的结果
    /// </summary>
    class SnapShot : ISnapShot
    {
        public SnapShot(IntPtr dbPtr)
        {
            this.dbPtr = dbPtr;
        }
        public void Init()
        {
            this.readop = new RocksDbSharp.ReadOptions();

            snapshotHandle = Native.Instance.rocksdb_create_snapshot(this.dbPtr);
            Native.Instance.rocksdb_readoptions_set_snapshot(readop.Handle, snapshotHandle);

            //this.snapshot = db.CreateSnapshot();
            //this.readop.SetSnapshot(this.snapshot);
            var _height = GetValue(LightDB.systemtable_info, "_height".ToBytes_UTF8Encode());
            if (_height == null || _height.type == DBValue.Type.Deleted)
            {
                this.DataHeight = 0;
            }
            else
            {
                this.DataHeight = GetValue(LightDB.systemtable_info, "_height".ToBytes_UTF8Encode()).AsUInt64();
            }
        }
        int refCount = 0;
        public IntPtr dbPtr;
        //public RocksDbSharp.RocksDb db;
        public RocksDbSharp.ReadOptions readop;
        public IntPtr snapshotHandle=IntPtr.Zero;
        //public RocksDbSharp.Snapshot snapshot;
        public UInt64 DataHeight
        {
            get;
            private set;
        }
        public void Dispose()
        {
            lock (this)
            {
                refCount--;
                if (refCount == 0 && snapshotHandle != IntPtr.Zero)
                {
                    Native.Instance.rocksdb_release_snapshot(this.dbPtr, snapshotHandle);
                    //snapshot.Dispose();
                    snapshotHandle = IntPtr.Zero;
                    readop = null;
                }
            }
        }
        /// <summary>
        /// 对snapshot的引用计数加锁，保证处理是线程安全的
        /// </summary>
        public void AddRef()
        {
            lock (this)
            {
                refCount++;
            }
        }
        public byte[] GetValueData(byte[] tableid, byte[] key)
        {
            byte[] finialkey = Helper.CalcKey(tableid, key);
            return Native.Instance.rocksdb_get(this.dbPtr, this.readop.Handle, finialkey, finialkey.LongLength, null);
            //(readOptions ?? DefaultReadOptions).Handle, key, keyLength, cf);

            //return this.db.Get(finialkey, null, readop);
        }
        public DBValue GetValue(byte[] tableid, byte[] key)
        {
            return DBValue.FromRaw(GetValueData(tableid, key));
        }
        public IEnumerable<byte[]> CreateKeyFinder(byte[] tableid, byte[] beginkey = null, byte[] endkey = null)
        {
            TableKeyFinder find = new TableKeyFinder(this, tableid, beginkey, endkey);
            return find;
        }
        public IEnumerator<byte[]> CreateKeyIterator(byte[] tableid, byte[] _beginkey = null, byte[] _endkey = null)
        {
            var beginkey = Helper.CalcKey(tableid, _beginkey);
            var endkey = Helper.CalcKey(tableid, _endkey);
            return new TableIterator(this, tableid, beginkey, endkey);
        }
        public TableInfo GetTableInfo(byte[] tableid)
        {
            var tablekey = Helper.CalcKey(tableid, null, SplitWord.TableInfo);
            var data = Native.Instance.rocksdb_get(this.dbPtr, this.readop.Handle, tablekey, tablekey.LongLength, null);
            if (data == null)
                return null;
            return TableInfo.FromRaw(DBValue.FromRaw(data).value);
        }
        public uint GetTableCount(byte[] tableid)
        {
            var tablekey = Helper.CalcKey(tableid, null, SplitWord.TableCount);
            var data = Native.Instance.rocksdb_get(this.dbPtr, this.readop.Handle, tablekey, tablekey.LongLength, null);
            return DBValue.FromRaw(data).AsUInt32();
        }
    }
}
