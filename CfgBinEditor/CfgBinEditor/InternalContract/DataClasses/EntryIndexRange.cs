namespace CfgBinEditor.InternalContract.DataClasses
{
    internal struct EntryIndexRange
    {
        public int Start { get; }
        public int End { get; }

        public EntryIndexRange(int start, int end)
        {
            Start = start;
            End = end;
        }
    }
}
