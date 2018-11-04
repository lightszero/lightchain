using System;
using System.Collections.Generic;
using System.Text;

namespace lightchain.db
{

    /// <summary>
    /// snapshot 快照，是rocksdb的重要功能，用快照来读取，无论此时数据如何被写入，都不会影响读取的结果
    /// </summary>
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
        public byte[] GetValueData(byte[] tableid, byte[] key)
        {
            byte[] finialkey = Helper.CalcKey(tableid, key);
            return this.db.Get(finialkey, null, readop);
        }
        public DBValue GetValue(byte[] tableid, byte[] key)
        {
            return DBValue.FromRaw(GetValueData(tableid, key));
        }
        public TableKeyFinder CreateKeyFinder(byte[] tableid, byte[] beginkey = null, byte[] endkey = null)
        {
            TableKeyFinder find = new TableKeyFinder(this, tableid, beginkey, endkey);
            return find;
        }
        public TableIterator CreateKeyIterator(byte[] tableid, byte[] _beginkey = null, byte[] _endkey = null)
        {
            var beginkey = Helper.CalcKey(tableid, _beginkey);
            var endkey = Helper.CalcKey(tableid, _endkey);
            return new TableIterator(this, tableid, beginkey, endkey);
        }
        public TableInfo GetTableInfo(byte[] tableid)
        {
            var tablekey = Helper.CalcKey(tableid, null, SplitWord.TableInfo);
            var data = this.db.Get(tablekey, null, readop);
            if (data == null)
                return null;
            return TableInfo.FromRaw(DBValue.FromRaw(data).value);
        }
        public uint GetTableCount(byte[] tableid)
        {
            var tablekey = Helper.CalcKey(tableid, null, SplitWord.TableCount);
            var data = this.db.Get(tablekey, null, readop);
            return DBValue.FromRaw(data).AsUInt32();
        }
    }
}
