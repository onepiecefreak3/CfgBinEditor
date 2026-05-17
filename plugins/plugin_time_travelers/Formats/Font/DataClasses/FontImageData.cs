using plugin_time_travelers.Formats.Font.Enums;
using plugin_time_travelers.Formats.Image.DataClasses;

namespace plugin_time_travelers.Formats.Font.DataClasses
{
    public class FontImageData
    {
        public PlatformType Platform { get; set; }
        public FontData Font { get; set; }
        public ImageData[] Images { get; set; }
    }
}
