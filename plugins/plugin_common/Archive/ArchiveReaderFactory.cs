using plugin_common.Archive.Enums;
using plugin_common.Archive.InternalContract;

namespace plugin_common.Archive
{
    public class ArchiveReaderFactory
    {
        public static IArchiveReader Create(ArchiveType type)
        {
            switch (type)
            {
                case ArchiveType.Xpck:
                    return new XpckReader();

                case ArchiveType.Xfsp:
                    return new XfspReader();

                default:
                    throw new InvalidOperationException($"Unknown archive type {type}.");
            }
        }
    }
}
