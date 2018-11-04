using System;
using System.Collections.Generic;
using System.Text;

namespace lightchain.db
{
    public enum WriteTaskOP
    {
        CreateTable,
        DeleteTable,
        PutValue,
        DeleteValue,
        Log,//log 暂时没有作用，仅仅提供存放个信息
    }
    public class WriteTaskItem
    {
        public WriteTaskOP op;
        public byte[] tableID;
        public byte[] key;
        public byte[] value;
    }

    public class WriteTask
    {
        /// <summary>
        /// write task 构造不需要啥
        /// </summary>
        public WriteTask()
        {
            items = new List<WriteTaskItem>();

        }
        public List<WriteTaskItem> items;


        public void CreateTable(TableInfo info)
        {
            if (info.tableid.Length < 2)
                throw new Exception("not allow too short table id.");
            items.Add(
                    new WriteTaskItem()
                    {
                        op = WriteTaskOP.CreateTable,
                        tableID = info.tableid,
                        key = null,
                        value = info.ToBytes()
                    }
                );
        }
        public void DeleteTable(byte[] tableid)
        {
            items.Add(
                     new WriteTaskItem()
                     {
                         op = WriteTaskOP.DeleteTable,
                         tableID = tableid,
                         key = null,
                         value = null
                     }
                 );
        }
        public void PutUnsafe(byte[] tableid, byte[] key, byte[] data)
        {
            items.Add(
                     new WriteTaskItem()
                     {
                         op = WriteTaskOP.PutValue,
                         tableID = tableid,
                         key = key,
                         value = data
                     }
                 );
        }
        public void Put(byte[] tableid, byte[] key, DBValue value)
        {
            PutUnsafe(tableid, key, value.ToBytes());
        }
        public void Delete(byte[] tableid, byte[] key)
        {
            items.Add(
                    new WriteTaskItem()
                    {
                        op = WriteTaskOP.PutValue,
                        tableID = tableid,
                        key = key,
                        value = null
                    }
                );
        }
    }
}
