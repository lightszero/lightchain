﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RocksDbSharp
{
    public abstract partial class Native
    {
        public static Native Instance;
        
        static Native()
        {
            if (RuntimeInformation.ProcessArchitecture == Architecture.X86 && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new Exception("Rocksdb on windows is not supported for 32 bit applications");
            Instance = NativeImport.Auto.Import<Native>("rocksdb", "5.17.0", true);
        }

        public Native()
        {
        }
    }
}
