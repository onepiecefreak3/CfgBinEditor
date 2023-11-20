using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.Level5.ConfigBinary.InternalContract.DataClasses
{
    internal struct CfgBinEntry
    {
        public uint crc32;
        public byte entryCount;
        public byte[] entryTypes;
        public uint[] entryValues;
    }
}
