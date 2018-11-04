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
            return new TableIterator(this, tablehead, beginkey, endkey);
        }
        public TableInfo GetTableInfo(byte[] tablehead)
        {
            var tablekey = Helper.CalcKey(tablehead, null, SplitWord.TableInfo);
            var data = this.db.Get(tablekey, null, readop);
            if (data == null)
                return null;
            return TableInfo.FromRaw(DBValue.FromRaw(data).value);
        }
        public uint GetTableCount(byte[] tablehead)
        {
            var tablekey = Helper.CalcKey(tablehead, null, SplitWord.TableCount);
            var data = this.db.Get(tablekey, null, readop);
            return DBValue.FromRaw(data).AsUInt32();
        }
    }
}
