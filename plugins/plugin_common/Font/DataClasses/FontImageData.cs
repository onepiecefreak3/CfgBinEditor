using plugin_common.Font.Enums;
using plugin_common.Image.DataClasses;

namespace plugin_common.Font.DataClasses
{
    public class FontImageData
    {
        public PlatformType Platform { get; set; }
        public FontData Font { get; set; }
        public ImageData[] Images { get; set; }
    }
}
