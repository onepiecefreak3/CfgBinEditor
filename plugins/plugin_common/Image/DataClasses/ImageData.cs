using Konnect.Contract.Plugin.File.Image;
using plugin_common.Font.DataClasses;

namespace plugin_common.Image.DataClasses
{
    public class ImageData
    {
        public FormatVersion Version { get; set; }

        public IImageFile Image { get; set; }
        public IImageFile[] Mipmaps { get; set; }

        public byte[]? LegacyData { get; set; }
    }
}
