using System;
using System.Collections.Generic;
using System.Text;

namespace lightchain.db
{
    public enum WriteFunc
    {
        Log,//忽略的操作
        CreateTable,//创建表格
        DeleteTable,//删除表格
        PutValue, //修改值
        DeleteValue,//删除值
    }
    public interface IWriteOp
    {
        WriteFunc func
        {
            get;
        }
        void Pack(System.IO.Stream stream);
    }
    public class WriteOp_Log : IWriteOp
    {
        public string loginfo;
        public WriteFunc func => WriteFunc.Log;

        public void Pack(System.IO.Stream stream)
        {
            throw new NotImplementedException();
        }
    }
    public class WriteOp_CreateTable : IWriteOp
    {
        public WriteOp_CreateTable(byte[] _tableid, string _tablename, string _tabledesc, DBValue.Type _keytype)
        {
            this.tableid= _tableid;
            this.tablename = _tablename;
            this.tabledesc = _tabledesc;
            this.keytype = _keytype;
        }
        public byte[] tableid;
        public string tablename;
        public string tabledesc;
        public DBValue.Type keytype;
        public WriteFunc func => WriteFunc.CreateTable;
        public void Pack(System.IO.Stream stream)
        {
            throw new NotImplementedException();
        }
        public static WriteOp_CreateTable UnPack(System.IO.Stream stream)
        {
            byte[] head = null;
            string name = null;
            string desc = null;
            DBValue.Type keytype = DBValue.Type.BigNumber;
            WriteOp_CreateTable table = new WriteOp_CreateTable(head, name, desc, keytype);
            return table;
        }
    }
    public class WriteOp_DeleteTable : IWriteOp
    {
        public byte[] tableid;
        public WriteFunc func => WriteFunc.DeleteTable;
        public void Pack(System.IO.Stream stream)
        {
            throw new NotImplementedException();
        }
    }
    public class WriteOp_PutValue : IWriteOp
    {
        public byte[] tableid;
        public byte[] key;
        public DBValue value;
        public WriteFunc func => WriteFunc.PutValue;
        public void Pack(System.IO.Stream stream)
        {
            throw new NotImplementedException();
        }
    }
    public class WriteOp_DeleteValue : IWriteOp
    {
        public byte[] tableid;
        public byte[] key;

        public WriteFunc func => WriteFunc.DeleteValue;
        public void Pack(System.IO.Stream stream)
        {
            throw new NotImplementedException();
        }
    }
    public class WriteBlock
    {
        public string blockid;
        public ulong SnapshotHeight;//提供快照高度，然后让WriteBatch 只有一个
        public List<IWriteOp> ops = new List<IWriteOp>();
        public void Pack(System.IO.Stream stream)
        {
        }
        public static WriteBlock UnPack(System.IO.Stream stream)
        {
            return null;
        }
        public byte[] ToBytes()
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            this.Pack(ms);
            return ms.ToArray();
        }
    }
}
