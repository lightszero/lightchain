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
    public class WriteBatch : IDisposable
    {
        public WriteBatch(RocksDbSharp.RocksDb db, SnapShot snapshot)
        {
            this.db = db;
            this.batch = new RocksDbSharp.WriteBatch();
            this.snapshot = snapshot;
            this.cache = new Dictionary<string, byte[]>();
        }
        RocksDbSharp.RocksDb db;
        SnapShot snapshot;
        public RocksDbSharp.WriteBatch batch;
        Dictionary<string, byte[]> cache;

        public void Dispose()
        {
            if (batch != null)
            {
                batch.Dispose();
                batch = null;
            }
        }
        public byte[] GetDataFinal(byte[] finalkey)
        {
            var hexkey = finalkey.ToString_Hex();
            if (cache.ContainsKey(hexkey))
            {
                return cache[hexkey];
            }
            else
            {
                var data = db.Get(finalkey, null, snapshot.readop);
                cache[hexkey] = data;
                return data;
            }
        }
        private void PutDataFinal(byte[] finalkey, byte[] value)
        {
            var hexkey = finalkey.ToString_Hex();
            cache[hexkey] = value;
            batch.Put(finalkey, value);
        }
        private void DeleteFinal(byte[] finalkey)
        {
            var hexkey = finalkey.ToString_Hex();
            cache.Remove(hexkey);
            batch.Delete(finalkey);
        }
        public void CreateTable(TableInfo info)
        {
            var finalkey = Helper.CalcKey(info.tablehead, null, SplitWord.TableInfo);
            var countkey = Helper.CalcKey(info.tablehead, null, SplitWord.TableCount);
            var data = GetDataFinal(finalkey);
            if (data != null && data[0] != (byte)DBValue.Type.Deleted)
            {
                throw new Exception("alread have that.");
            }
            var value = DBValue.FromValue(DBValue.Type.Bytes, info.ToBytes());
            PutDataFinal(finalkey, value.ToBytes());
            PutDataFinal(countkey, DBValue.FromValue(DBValue.Type.UINT32, (UInt32)0).ToBytes());
        }
        public void DeleteTable(byte[] tablehead, bool makeTag = false)
        {
            var finalkey = Helper.CalcKey(tablehead, null, SplitWord.TableInfo);
            var countkey = Helper.CalcKey(tablehead, null, SplitWord.TableCount);
            var vdata = GetDataFinal(finalkey);
            if (vdata != null && vdata[0] != (byte)DBValue.Type.Deleted)
            {
                if (makeTag)
                {
                    PutDataFinal(finalkey, DBValue.DeletedValue.ToBytes());
                    PutDataFinal(countkey, DBValue.DeletedValue.ToBytes());
                }
                else
                {
                    DeleteFinal(finalkey);
                    DeleteFinal(countkey);
                }
            }
            else//数据不存在
            {
                if (makeTag)
                {
                    PutDataFinal(finalkey, DBValue.DeletedValue.ToBytes());
                    PutDataFinal(countkey, DBValue.DeletedValue.ToBytes());
                }
            }
        }
        public void PutUnsafe(byte[] tablehead, byte[] key, byte[] data)
        {
            var finalkey = Helper.CalcKey(tablehead, key);
            var countkey = Helper.CalcKey(tablehead, null, SplitWord.TableCount);
            var countdata = GetDataFinal(countkey);
            UInt32 count = 0;
            if (countdata != null)
            {
                count = DBValue.FromRaw(countdata).AsUInt32();
            }
            count++;

            PutDataFinal(finalkey, data);
            PutDataFinal(countkey, DBValue.FromValue(DBValue.Type.UINT32, count).ToBytes());
        }

        public void Put(byte[] tablehead, byte[] key, DBValue value)
        {
            PutUnsafe(tablehead, key, value.ToBytes());
        }
        public void Delete(byte[] tablehead, byte[] key, bool makeTag = false)
        {
            var finalkey = Helper.CalcKey(tablehead, key);

            var countkey = Helper.CalcKey(tablehead, null, SplitWord.TableCount);
            var countdata = GetDataFinal(countkey);
            UInt32 count = 0;
            if (countdata != null)
            {
                count = DBValue.FromRaw(countdata).AsUInt32();
            }

            var vdata = GetDataFinal(finalkey);
            if (vdata != null && vdata[0] != (byte)DBValue.Type.Deleted)
            {
                if (makeTag)
                {
                    PutDataFinal(finalkey, DBValue.DeletedValue.ToBytes());
                }
                else
                {
                    DeleteFinal(finalkey);
                }
                count--;
                PutDataFinal(countkey, DBValue.FromValue(DBValue.Type.UINT32, count).ToBytes());
            }
            else//数据不存在
            {
                if (makeTag)
                {
                    PutDataFinal(finalkey, DBValue.DeletedValue.ToBytes());
                }
            }

        }
    }



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
