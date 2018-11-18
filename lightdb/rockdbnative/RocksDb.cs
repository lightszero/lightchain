using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Transitional;

namespace RocksDbSharp
{
    public class RocksDb : IDisposable
    {
        internal static ReadOptions DefaultReadOptions { get; } = new ReadOptions();
        internal static OptionsHandle DefaultOptions { get; } = new DbOptions();
        internal static Encoding DefaultEncoding => Encoding.UTF8;
        //private Dictionary<string, ColumnFamilyHandleInternal> columnFamilies;

        // Managed references to unmanaged resources that need to live at least as long as the db
        //internal dynamic References { get; } = new ExpandoObject();

        public IntPtr Handle { get; protected set; }

        public RocksDb(IntPtr handle)
        {
            this.Handle = handle;
        }

        public void Dispose()
        {

            Native.Instance.rocksdb_close(Handle);
        }

        public static RocksDb Open(OptionsHandle options, string path)
        {
            IntPtr db = Native.Instance.rocksdb_open(options.Handle, path);
            return new RocksDb(db);
        }

        public static RocksDb OpenReadOnly(OptionsHandle options, string path, bool errorIfLogFileExists)
        {
            IntPtr db = Native.Instance.rocksdb_open_for_read_only(options.Handle, path, errorIfLogFileExists);
            return new RocksDb(db);
        }


        public void SetOptions(IEnumerable<KeyValuePair<string, string>> options)
        {
            var keys = options.Select(e => e.Key).ToArray();
            var values = options.Select(e => e.Value).ToArray();
            Native.Instance.rocksdb_set_options(Handle, keys.Length, keys, values);
        }

        public string Get(string key, ColumnFamilyHandle cf = null, ReadOptions readOptions = null, Encoding encoding = null)
        {
            return Native.Instance.rocksdb_get(Handle, (readOptions ?? DefaultReadOptions).Handle, key, cf, encoding ?? DefaultEncoding);
        }

        public byte[] Get(byte[] key, ColumnFamilyHandle cf = null, ReadOptions readOptions = null)
        {
            return Get(key, key.GetLongLength(0), cf, readOptions);
        }

        public byte[] Get(byte[] key, long keyLength, ColumnFamilyHandle cf = null, ReadOptions readOptions = null)
        {
            return Native.Instance.rocksdb_get(Handle, (readOptions ?? DefaultReadOptions).Handle, key, keyLength, cf);
        }

        public long Get(byte[] key, byte[] buffer, long offset, long length, ColumnFamilyHandle cf = null, ReadOptions readOptions = null)
        {
            return Get(key, key.GetLongLength(0), buffer, offset, length, cf, readOptions);
        }

        public long Get(byte[] key, long keyLength, byte[] buffer, long offset, long length, ColumnFamilyHandle cf = null, ReadOptions readOptions = null)
        {
            unsafe
            {
                var ptr = Native.Instance.rocksdb_get(Handle, (readOptions ?? DefaultReadOptions).Handle, key, keyLength, out long valLength, cf);
                valLength = Math.Min(length, valLength);
                Marshal.Copy(ptr, buffer, (int)offset, (int)valLength);
                Native.Instance.rocksdb_free(ptr);
                return valLength;
            }
        }

        public KeyValuePair<byte[], byte[]>[] MultiGet(byte[][] keys, ColumnFamilyHandle[] cf = null, ReadOptions readOptions = null)
        {
            return Native.Instance.rocksdb_multi_get(Handle, (readOptions ?? DefaultReadOptions).Handle, keys);
        }

        public KeyValuePair<string, string>[] MultiGet(string[] keys, ColumnFamilyHandle[] cf = null, ReadOptions readOptions = null)
        {
            return Native.Instance.rocksdb_multi_get(Handle, (readOptions ?? DefaultReadOptions).Handle, keys);
        }




        public Iterator NewIterator(ColumnFamilyHandle cf = null, ReadOptions readOptions = null)
        {
            IntPtr iteratorHandle = cf == null
                ? Native.Instance.rocksdb_create_iterator(Handle, (readOptions ?? DefaultReadOptions).Handle)
                : Native.Instance.rocksdb_create_iterator_cf(Handle, (readOptions ?? DefaultReadOptions).Handle, cf.Handle);
            // Note: passing in read options here only to ensure that it is not collected before the iterator
            return new Iterator(iteratorHandle, readOptions);
        }

        public Iterator[] NewIterators(ColumnFamilyHandle[] cfs, ReadOptions[] readOptions)
        {
            throw new NotImplementedException("TODO: Implement NewIterators()");
            // See rocksdb_create_iterators
        }

        //public Snapshot CreateSnapshot()
        //{
        //    IntPtr snapshotHandle = Native.Instance.rocksdb_create_snapshot(Handle);
        //    return new Snapshot(Handle, snapshotHandle);
        //}



        public string GetProperty(string propertyName)
        {
            return Native.Instance.rocksdb_property_value_string(Handle, propertyName);
        }

        public string GetProperty(string propertyName, ColumnFamilyHandle cf)
        {
            return Native.Instance.rocksdb_property_value_cf_string(Handle, cf.Handle, propertyName);
        }

        public void IngestExternalFiles(string[] files, IngestExternalFileOptions ingestOptions, ColumnFamilyHandle cf = null)
        {
            if (cf == null)
                Native.Instance.rocksdb_ingest_external_file(Handle, files, (ulong)files.Length, ingestOptions.Handle);
            else
                Native.Instance.rocksdb_ingest_external_file_cf(Handle, cf.Handle, files, (ulong)files.Length, ingestOptions.Handle);
        }

        public void CompactRange(byte[] start, byte[] limit, ColumnFamilyHandle cf = null)
        {
            if (cf == null)
                Native.Instance.rocksdb_compact_range(Handle, start, start.GetLongLength(0), limit, limit.GetLongLength(0));
            else
                Native.Instance.rocksdb_compact_range_cf(Handle, cf.Handle, start, start.GetLongLength(0), limit, limit.GetLongLength(0));
        }

        public void CompactRange(string start, string limit, ColumnFamilyHandle cf = null, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;
            CompactRange(encoding.GetBytes(start), encoding.GetBytes(limit), cf);
        }
    }
}
