using plugin_time_travelers.Formats.Archive.DataClasses;

namespace plugin_time_travelers.Formats.Archive.InternalContract
{
    public interface IArchiveReader
    {
        ArchiveData Read(Stream input);
    }
}
