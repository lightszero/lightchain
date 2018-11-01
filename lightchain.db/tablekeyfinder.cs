using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace lightchain.db
{
    public class TableKeyFinder : IEnumerable<byte[]>
    {
        public TableKeyFinder(SnapShot _snapshot, byte[] tablehead, byte[] _beginkey, byte[] _endkey)
        {
            this.snapshot = _snapshot;
            this.beginkey = Helper.CalcKey(tablehead, _beginkey);
            this.endkey = Helper.CalcKey(tablehead, _endkey);
        }
        SnapShot snapshot;
        byte[] beginkey;
        byte[] endkey;
        public IEnumerator<byte[]> GetEnumerator()
        {
            return new TableIterator(snapshot, beginkey, endkey);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    public class TableIterator : IEnumerator<byte[]>
    {
        public TableIterator(SnapShot snapshot, byte[] _beginkey, byte[] _endkey)
        {
            this.it = snapshot.db.NewIterator(null, snapshot.readop);
            this.beginkey = _beginkey;
            this.endkey = _endkey;
            //this.Reset();

        }
        bool bInit = false;
        RocksDbSharp.Iterator it;
        byte[] beginkey;
        byte[] endkey;
        public byte[] Current
        {
            get
            {
                if (this.Vaild)
                    return it.Key();
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
            if (data.Length < this.endkey.Length)
                return false;
            for (var i = 0; i < endkey.Length; i++)
            {
                if (data[i] != this.endkey[i])
                    return false;
            }
            return true;
        }
        public bool MoveNext()
        {
            if (bInit == false)
            {
                bInit = true;
                it.Seek(beginkey);
            }
            else
            {
                it.Next();
            }
            if (it.Valid() == false)
                return false;
            this.Vaild = TestVaild(it.Key());
            return true;
        }

        public void Reset()
        {
            it.Seek(beginkey);
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
