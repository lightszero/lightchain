using System;
using System.Collections.Generic;
using System.Text;

namespace lightchain.db
{
    public interface ISnapShot : IDisposable
    {
        byte[] GetValueData(byte[] tableid, byte[] key);
        DBValue GetValue(byte[] tableid, byte[] key);
        IEnumerable<byte[]> CreateKeyFinder(byte[] tableid, byte[] beginkey = null, byte[] endkey = null);
        IEnumerator<byte[]> CreateKeyIterator(byte[] tableid, byte[] _beginkey = null, byte[] _endkey = null);
        TableInfo GetTableInfo(byte[] tableid);
        uint GetTableCount(byte[] tableid);
    }
}
