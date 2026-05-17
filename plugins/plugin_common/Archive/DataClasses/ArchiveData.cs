namespace plugin_common.Archive.DataClasses
{
    public class ArchiveData
    {
        public byte Type { get; set; }
        public IList<NamedArchiveEntry> Files { get; set; }
    }
}
