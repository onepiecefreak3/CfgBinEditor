namespace Logic.Domain.Level5Management.Rdbn.InternalContract
{
    internal struct RdbnRootEntry
    {
        public short typeIndex;
        public short unk1;
        public int valueOffset;
        public int valueSize;
        public int valueCount;
        public uint nameHash;
    }
}
