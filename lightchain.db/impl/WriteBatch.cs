using System;
using System.Collections.Generic;
using System.Text;

namespace lightchain.db
{
    /// <summary>
    /// WriteBatch 写入批，是个很基本的功能，不应该对外暴露
    /// </summary>
    class WriteBatch : IDisposable
    {
        public WriteBatch(IntPtr dbptr, SnapShot snapshot)
        {
            this.dbPtr = dbptr;
            this.batchptr = RocksDbSharp.Native.Instance.rocksdb_writebatch_create();
            //this.batch = new RocksDbSharp.WriteBatch();
            this.snapshot = snapshot;
            this.cache = new Dictionary<string, byte[]>();
        }
        //RocksDbSharp.RocksDb db;
        public IntPtr dbPtr;
        SnapShot snapshot;
        //public RocksDbSharp.WriteBatch batch;
        public IntPtr batchptr;
        Dictionary<string, byte[]> cache;

        public void Dispose()
        {
            if (batchptr != IntPtr.Zero)
            {
                RocksDbSharp.Native.Instance.rocksdb_writebatch_destroy(batchptr);
                batchptr = IntPtr.Zero;
                //batch.Dispose();
                //batch = null;
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
                var data = RocksDbSharp.Native.Instance.rocksdb_get(dbPtr, snapshot.readop.Handle, finalkey, finalkey.Length);
                //db.Get(finalkey, null, snapshot.readop);
                cache[hexkey] = data;
                return data;
            }
        }
        private void PutDataFinal(byte[] finalkey, byte[] value)
        {
            var hexkey = finalkey.ToString_Hex();
            cache[hexkey] = value;
            RocksDbSharp.Native.Instance.rocksdb_writebatch_put(batchptr, finalkey, (ulong)finalkey.Length, value, (ulong)value.Length);
            //batch.Put(finalkey, value);
        }
        private void DeleteFinal(byte[] finalkey)
        {
            var hexkey = finalkey.ToString_Hex();
            cache.Remove(hexkey);
            RocksDbSharp.Native.Instance.rocksdb_writebatch_delete(batchptr, finalkey, (ulong)finalkey.Length);
            //batch.Delete(finalkey);
        }
        public void CreateTable(TableInfo info)
        {
            var finalkey = Helper.CalcKey(info.tableid, null, SplitWord.TableInfo);
            var countkey = Helper.CalcKey(info.tableid, null, SplitWord.TableCount);
            var data = GetDataFinal(finalkey);
            if (data != null && data[0] != (byte)DBValue.Type.Deleted)
            {
                throw new Exception("alread have that.");
            }
            var value = DBValue.FromValue(DBValue.Type.Bytes, info.ToBytes());
            PutDataFinal(finalkey, value.ToBytes());
            PutDataFinal(countkey, DBValue.FromValue(DBValue.Type.UINT32, (UInt32)0).ToBytes());
        }
        public void CreateTable(byte[] tableid, byte[] finaldata)
        {
            var finalkey = Helper.CalcKey(tableid, null, SplitWord.TableInfo);
            var countkey = Helper.CalcKey(tableid, null, SplitWord.TableCount);
            var data = GetDataFinal(finalkey);
            if (data != null && data[0] != (byte)DBValue.Type.Deleted)
            {
                throw new Exception("alread have that.");
            }
            //var value = DBValue.FromValue(DBValue.Type.Bytes, infodata);
            PutDataFinal(finalkey, finaldata);
            PutDataFinal(countkey, DBValue.FromValue(DBValue.Type.UINT32, (UInt32)0).ToBytes());
        }
        public void DeleteTable(byte[] tableid, bool makeTag = false)
        {
            var finalkey = Helper.CalcKey(tableid, null, SplitWord.TableInfo);
            //var countkey = Helper.CalcKey(tableid, null, SplitWord.TableCount);
            var vdata = GetDataFinal(finalkey);
            if (vdata != null && vdata[0] != (byte)DBValue.Type.Deleted)
            {
                if (makeTag)
                {
                    PutDataFinal(finalkey, DBValue.DeletedValue.ToBytes());
                    //PutDataFinal(countkey, DBValue.DeletedValue.ToBytes());
                }
                else
                {
                    DeleteFinal(finalkey);
                    //DeleteFinal(countkey);
                }
            }
            else//数据不存在
            {
                if (makeTag)
                {
                    PutDataFinal(finalkey, DBValue.DeletedValue.ToBytes());
                    //PutDataFinal(countkey, DBValue.DeletedValue.ToBytes());
                }
            }
        }
        public void PutUnsafe(byte[] tableid, byte[] key, byte[] finaldata)
        {
            var finalkey = Helper.CalcKey(tableid, key);
            var countkey = Helper.CalcKey(tableid, null, SplitWord.TableCount);
            var countdata = GetDataFinal(countkey);
            UInt32 count = 0;
            if (countdata != null)
            {
                count = DBValue.FromRaw(countdata).AsUInt32();
            }
            var vdata = GetDataFinal(finalkey);
            if (vdata == null || vdata[0] == (byte)DBValue.Type.Deleted)
            {
                count++;
            }
            PutDataFinal(finalkey, finaldata);
            PutDataFinal(countkey, DBValue.FromValue(DBValue.Type.UINT32, count).ToBytes());
        }

        public void Put(byte[] tableid, byte[] key, DBValue value)
        {
            PutUnsafe(tableid, key, value.ToBytes());
        }
        public void Delete(byte[] tableid, byte[] key, bool makeTag = false)
        {
            var finalkey = Helper.CalcKey(tableid, key);

            var countkey = Helper.CalcKey(tableid, null, SplitWord.TableCount);
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


}
