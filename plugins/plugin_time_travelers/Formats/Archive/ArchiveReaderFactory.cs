using plugin_time_travelers.Formats.Archive.Enums;
using plugin_time_travelers.Formats.Archive.InternalContract;

namespace plugin_time_travelers.Formats.Archive
{
    internal class ArchiveReaderFactory
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
