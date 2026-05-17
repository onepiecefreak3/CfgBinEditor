namespace plugin_time_travelers.Formats.Archive.DataClasses
{
    public class ArchiveData
    {
        public byte Type { get; set; }
        public IList<NamedArchiveEntry> Files { get; set; }
    }
}
