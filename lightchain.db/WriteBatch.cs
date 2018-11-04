using System;
using System.Collections.Generic;
using System.Text;

namespace lightchain.db
{
    /// <summary>
    /// WriteBatch 写入批，是个很基本的功能，不应该对外暴露
    /// </summary>
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
            //var countkey = Helper.CalcKey(tablehead, null, SplitWord.TableCount);
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


}
