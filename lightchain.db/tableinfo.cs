using System;
using System.Collections.Generic;
using System.Text;

namespace lightchain.db
{
    public class TableInfo
    {
        public TableInfo(byte[] _tablehead, string _tablename, string _tabledesc, DBValue.Type _keytype)
        {
            this.tablehead = _tablehead;
            this.tablename = _tablename;
            this.tabledesc = _tabledesc;
            this.keytype = _keytype;
        }
        public byte[] tablehead;
        public string tablename;
        public string tabledesc;
        public DBValue.Type keytype;

        public void Pack(System.IO.Stream stream)
        {
            throw new NotImplementedException();
        }
        public static TableInfo UnPack(System.IO.Stream stream)
        {
            byte[] head = null;
            string name = null;
            string desc = null;
            DBValue.Type keytype = DBValue.Type.BigNumber;
            TableInfo table = new TableInfo(head, name, desc, keytype);
            return table;
        }
        public byte[] ToBytes()
        {
            using (var ms = new System.IO.MemoryStream())
            {
                Pack(ms);
                return ms.ToArray();
            }
        }
        public static TableInfo FromRaw(byte[] data)
        {
            using (var ms = new System.IO.MemoryStream(data))
            {
                return TableInfo.UnPack(ms);
            }
        }
    }
}
