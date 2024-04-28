using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValueType = Logic.Domain.Level5.Contract.DataClasses.ValueType;

namespace Logic.Domain.Level5.ConfigBinary.InternalContract.DataClasses
{
    internal struct CfgBinEntry
    {
        public uint crc32;
        public byte entryCount;
        public ValueType[] entryTypes;
        public long[] entryValues;
    }
}
