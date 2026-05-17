using plugin_common.Archive.DataClasses;

namespace plugin_common.Archive.InternalContract
{
    public interface IArchiveReader
    {
        ArchiveData Read(Stream input);
    }
}
