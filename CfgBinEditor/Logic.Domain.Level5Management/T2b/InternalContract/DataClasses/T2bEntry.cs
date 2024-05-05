using ValueType = Logic.Domain.Level5Management.Contract.DataClasses.ValueType;

namespace Logic.Domain.Level5Management.T2b.InternalContract.DataClasses
{
    internal struct T2bEntry
    {
        public uint crc32;
        public byte entryCount;
        public ValueType[] entryTypes;
        public long[] entryValues;
    }
}
