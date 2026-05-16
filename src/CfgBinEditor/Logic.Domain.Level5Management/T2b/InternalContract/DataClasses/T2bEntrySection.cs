using Logic.Domain.Level5Management.Contract.DataClasses;

namespace Logic.Domain.Level5Management.T2b.InternalContract.DataClasses
{
    internal struct T2bEntrySection
    {
        public T2bEntry[] Entries { get; set; }
        public long StringOffset { get; set; }
        public int StringSize { get; set; }
        public ValueLength ValueLength { get; set; }
    }
}
