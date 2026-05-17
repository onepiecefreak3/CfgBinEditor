namespace plugin_common.Archive.DataClasses
{
    public struct XfspEntry
    {
        public ushort hash;
        public ushort nameOffset;

        public ushort fileOffsetLower;
        public ushort fileSizeLower;
        public byte fileOffsetUpper;
        public byte fileSizeUpper;
    }
}
