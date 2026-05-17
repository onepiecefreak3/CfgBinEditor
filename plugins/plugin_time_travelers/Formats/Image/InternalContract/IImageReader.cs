using plugin_time_travelers.Formats.Image.DataClasses;

namespace plugin_time_travelers.Formats.Image.InternalContract
{
    public interface IImageReader
    {
        RawImageData Read(Stream input);
    }
}
