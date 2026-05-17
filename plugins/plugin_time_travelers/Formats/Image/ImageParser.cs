using plugin_time_travelers.Formats.Image.DataClasses;
using plugin_time_travelers.Formats.Image.InternalContract;

namespace plugin_time_travelers.Formats.Image
{
    internal class ImageParser
    {
        public static ImageData Parse(Stream input)
        {
            int imageVersion = ImageVersionReader.Peek(input);

            IImageReader imageReader = ImageReaderFactory.Create(imageVersion);
            RawImageData imageData = imageReader.Read(input);

            return ImageDecoder.Decode(imageData);
        }
    }
}
