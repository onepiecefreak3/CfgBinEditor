using Konnect.Contract.Plugin.File.Image;
using plugin_time_travelers.Formats.Font.DataClasses;

namespace plugin_time_travelers.Formats.Image.DataClasses
{
    public class ImageData
    {
        public FormatVersion Version { get; set; }

        public IImageFile Image { get; set; }
        public IImageFile[] Mipmaps { get; set; }

        public byte[]? LegacyData { get; set; }
    }
}
