namespace plugin_time_travelers.Formats.Font.DataClasses
{
    public struct Fnt01Header
    {
        public string magic;
        public int const1;
        public short largeCharHeight;
        public short smallCharHeight;
        public ushort largeEscapeCharacterIndex;
        public ushort smallEscapeCharacterIndex;
        public long zero0;

        public short charSizeOffset;
        public short charSizeCount;
        public short largeCharOffset;
        public short largeCharCount;
        public short smallCharOffset;
        public short smallCharCount;
    }
}
