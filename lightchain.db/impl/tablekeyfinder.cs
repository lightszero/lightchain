﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace lightchain.db
{
    class TableKeyFinder : IEnumerable<byte[]>
    {
        public TableKeyFinder(SnapShot _snapshot, byte[] _tableid, byte[] _beginkey, byte[] _endkey)
        {
            this.snapshot = _snapshot;
            this.tableid = _tableid;
            this.beginkeyfinal = Helper.CalcKey(_tableid, _beginkey);
            this.endkeyfinal = Helper.CalcKey(_tableid, _endkey);
        }
        SnapShot snapshot;
        byte[] tableid;
        byte[] beginkeyfinal;
        byte[] endkeyfinal;
        public IEnumerator<byte[]> GetEnumerator()
        {
            return new TableIterator(snapshot, tableid, beginkeyfinal, endkeyfinal);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    class TableIterator : IEnumerator<byte[]>
    {
        public TableIterator(SnapShot snapshot, byte[] _tableid, byte[] _beginkeyfinal, byte[] _endkeyfinal)
        {
            this.it = snapshot.db.NewIterator(null, snapshot.readop);
            this.tableid = _tableid;
            this.beginkeyfinal = _beginkeyfinal;
            this.endkeyfinal = _endkeyfinal;
            //this.Reset();

        }
        bool bInit = false;
        RocksDbSharp.Iterator it;
        byte[] tableid;
        byte[] beginkeyfinal;
        byte[] endkeyfinal;
        public byte[] Current
        {
            get
            {
                if (this.Vaild)
                    return it.Key().Skip(this.tableid.Length + 2).ToArray();
                else
                    return null;
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public bool Vaild
        {
            get;
            private set;
        }
        public bool TestVaild(byte[] data)
        {
            if (data.Length < this.endkeyfinal.Length)
                return false;
            for (var i = 0; i < endkeyfinal.Length; i++)
            {
                if (data[i] != this.endkeyfinal[i])
                    return false;
            }
            return true;
        }
        public bool MoveNext()
        {
            if (bInit == false)
            {
                bInit = true;
                it.Seek(beginkeyfinal);
            }
            else
            {
                it.Next();
            }
            if (it.Valid() == false)
                return false;
            this.Vaild = TestVaild(it.Key());
            return this.Vaild;
        }

        public void Reset()
        {
            it.Seek(beginkeyfinal);
            bInit = false;
            this.Vaild = false;
        }

        public void Dispose()
        {
            it.Dispose();
            it = null;
        }
    }

}
