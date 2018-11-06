using System;
using System.Collections.Generic;
using System.Text;

namespace LightDB
{
    public interface ISnapShot : IDisposable
    {
        /// <summary>
        /// 得到数据高度
        /// </summary>
        /// <returns></returns>
        UInt64 DataHeight
        {
            get;
        }
        byte[] GetValueData(byte[] tableid, byte[] key);
        DBValue GetValue(byte[] tableid, byte[] key);
        IEnumerable<byte[]> CreateKeyFinder(byte[] tableid, byte[] beginkey = null, byte[] endkey = null);
        IEnumerator<byte[]> CreateKeyIterator(byte[] tableid, byte[] _beginkey = null, byte[] _endkey = null);
        TableInfo GetTableInfo(byte[] tableid);
        uint GetTableCount(byte[] tableid);
    }
}
