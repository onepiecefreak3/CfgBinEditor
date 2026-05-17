using plugin_common.Image.DataClasses;

namespace plugin_common.Image.InternalContract
{
    public interface IImageReader
    {
        RawImageData Read(Stream input);
    }
}
