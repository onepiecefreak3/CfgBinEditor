namespace Logic.Domain.Level5Management.Rdbn.InternalContract
{
    internal struct RdbnHeader
    {
        public uint magic;
        public short headerSize;
        public int version;
        public short dataOffset;
        public int dataSize;

        public short typeOffset;
        public short typeCount;
        public short fieldOffset;
        public short fieldCount;
        public short rootOffset;
        public short rootCount;
        public short stringHashOffset;
        public short stringOffsetsOffset;
        public short hashCount;
        public short valueOffset;
        public int stringOffset;
    }
}
