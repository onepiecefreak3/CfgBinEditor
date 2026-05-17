using Kanvas;
using Kanvas.Encoding;
using Komponent.Contract.Enums;
using Konnect.Contract.DataClasses.Plugin.File.Image;
using Konnect.Contract.Plugin.File.Image;
using Konnect.Plugin.File.Image;
using plugin_time_travelers.Formats.Font.Enums;
using plugin_time_travelers.Formats.Image.DataClasses;
using SixLabors.ImageSharp;
using ByteOrder = Komponent.Contract.Enums.ByteOrder;

namespace plugin_time_travelers.Formats.Image
{
    internal class ImageDecoder
    {
        public static ImageData Decode(RawImageData imageData)
        {
            EncodingDefinition definition = GetEncodingDefinition(imageData.Version.Platform);
            return GetImage(imageData, definition);
        }

        private static ImageData GetImage(RawImageData imageData, EncodingDefinition definition)
        {
            return new ImageData
            {
                Version = imageData.Version,
                Image = GetMainImage(imageData, definition),
                Mipmaps = GetMipMaps(imageData, definition),
                LegacyData = imageData.LegacyData
            };
        }

        private static IImageFile GetMainImage(RawImageData imageData, EncodingDefinition definition)
        {
            var imageInfo = new ImageFileInfo
            {
                BitDepth = imageData.BitDepth,
                ImageSize = new Size(imageData.Width, imageData.Height),
                ImageFormat = imageData.Format,
                ImageData = imageData.Data,
                RemapPixels = options => new ImageSwizzle(options, imageData.Version.Platform)
            };

            if (imageData.PaletteFormat >= 0)
            {
                imageInfo.PaletteBitDepth = imageData.PaletteBitDepth;
                imageInfo.PaletteFormat = imageData.PaletteFormat;
                imageInfo.PaletteData = imageData.PaletteData;
            }

            return new ImageFile(imageInfo, definition);
        }

        private static IImageFile[] GetMipMaps(RawImageData imageData, EncodingDefinition definition)
        {
            if ((imageData.MipMapData?.Length ?? 0) <= 0)
                return Array.Empty<IImageFile>();

            var result = new List<IImageFile>();

            int width = imageData.Width >> 1;
            int height = imageData.Height >> 1;

            foreach (byte[] mipmap in imageData.MipMapData!)
            {
                var imageInfo = new ImageFileInfo
                {
                    BitDepth = imageData.BitDepth,
                    ImageSize = new Size(width, height),
                    ImageFormat = imageData.Format,
                    ImageData = mipmap,
                    RemapPixels = options => new ImageSwizzle(options, imageData.Version.Platform)
                };

                if (imageData.PaletteFormat >= 0)
                {
                    imageInfo.PaletteBitDepth = imageData.PaletteBitDepth;
                    imageInfo.PaletteFormat = imageData.PaletteFormat;
                    imageInfo.PaletteData = imageData.PaletteData;
                }

                result.Add(new ImageFile(imageInfo, definition));

                width = imageData.Width >> 1;
                height = imageData.Height >> 1;
            }

            return result.ToArray();
        }

        private static EncodingDefinition GetEncodingDefinition(PlatformType platform)
        {
            switch (platform)
            {
                case PlatformType.Ctr:
                    return CreateCtrFormats();

                case PlatformType.Psp:
                    return CreatePspFormats();

                case PlatformType.PsVita:
                    return CreateVitaFormats();

                default:
                    throw new InvalidOperationException($"Unsupported platform {platform} for image.");
            }
        }

        private static EncodingDefinition CreateCtrFormats()
        {
            var result = new EncodingDefinition();

            result.AddColorEncoding(0x00, ImageFormats.Rgba8888());
            result.AddColorEncoding(0x01, ImageFormats.Rgba4444());
            result.AddColorEncoding(0x02, ImageFormats.Rgba5551());
            result.AddColorEncoding(0x03, new Rgba(8, 8, 8, "BGR"));
            result.AddColorEncoding(0x04, ImageFormats.Rgb565());

            result.AddColorEncoding(0x0B, ImageFormats.La88());
            result.AddColorEncoding(0x0C, ImageFormats.La44());
            result.AddColorEncoding(0x0D, ImageFormats.L8());
            result.AddColorEncoding(0x0E, ImageFormats.L4());
            result.AddColorEncoding(0x0F, ImageFormats.A8());
            result.AddColorEncoding(0x10, ImageFormats.A4());

            result.AddColorEncoding(0x1B, ImageFormats.Etc1(true));
            result.AddColorEncoding(0x1C, ImageFormats.Etc1(true));
            result.AddColorEncoding(0x1D, ImageFormats.Etc1A4(true));

            return result;
        }

        private static EncodingDefinition CreatePspFormats()
        {
            var result = new EncodingDefinition();

            result.AddPaletteEncoding(0x00, ImageFormats.Rgba8888(ByteOrder.BigEndian));
            result.AddPaletteEncoding(0x01, new Rgba(4, 4, 4, 4, "ARGB"));
            result.AddPaletteEncoding(0x02, new Rgba(5, 5, 5, 1, "ABGR"));

            result.AddColorEncoding(0x00, ImageFormats.Rgba8888(ByteOrder.BigEndian));
            result.AddIndexEncoding(0x11, ImageFormats.I8(), new[] { 0, 1, 2 });
            result.AddIndexEncoding(0x13, ImageFormats.I8(), new[] { 0, 1, 2 });
            result.AddIndexEncoding(0x17, ImageFormats.I4(BitOrder.LeastSignificantBitFirst), new[] { 0, 1, 2 });

            return result;
        }

        private static EncodingDefinition CreateVitaFormats()
        {
            var result = new EncodingDefinition();

            result.AddColorEncoding(0x03, ImageFormats.Rgb888());
            result.AddColorEncoding(0x1E, ImageFormats.Dxt1());

            return result;
        }
    }
}
