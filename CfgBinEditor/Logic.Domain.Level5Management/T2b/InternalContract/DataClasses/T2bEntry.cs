using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValueType = Logic.Domain.Level5Management.Contract.DataClasses.ValueType;

namespace Logic.Domain.Level5Management.ConfigBinary.InternalContract.DataClasses
{
    internal struct T2bEntry
    {
        public uint crc32;
        public byte entryCount;
        public Contract.DataClasses.ValueType[] entryTypes;
        public long[] entryValues;
    }
}
