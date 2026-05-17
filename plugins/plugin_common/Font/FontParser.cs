using plugin_common.Archive;
using plugin_common.Archive.DataClasses;
using plugin_common.Archive.Enums;
using plugin_common.Archive.InternalContract;
using plugin_common.Font.DataClasses;
using plugin_common.Font.Enums;
using plugin_common.Font.InternalContract;
using plugin_common.Image;
using plugin_common.Image.DataClasses;

namespace plugin_common.Font
{
    public class FontParser
    {
        public static FontImageData? Parse(Stream input)
        {
            ArchiveData archiveData = ReadFiles(input);

            FontData? fontData = GetFontData(archiveData.Files);
            if (fontData == null)
                return null;

            ImageData[] glyphImages = GetGlyphImages(archiveData.Files);
            PlatformType platform = GetPlatform(fontData, glyphImages);

            return new FontImageData
            {
                Platform = platform,
                Font = fontData,
                Images = glyphImages
            };
        }

        private static ArchiveData ReadFiles(Stream input)
        {
            ArchiveType archiveType = ArchiveTypeReader.Peek(input);
            IArchiveReader reader = ArchiveReaderFactory.Create(archiveType);

            return reader.Read(input);
        }

        private static FontData? GetFontData(IList<NamedArchiveEntry> files)
        {
            NamedArchiveEntry? fntFile = files.FirstOrDefault(f => f.Name == "FNT.bin");
            if (fntFile == null)
                return null;

            int fontVersion = FontVersionReader.Peek(fntFile.Content);
            IFontReader fontReader = FontReaderFactory.Create(fontVersion);

            return fontReader.Read(fntFile.Content);
        }

        private static ImageData[] GetGlyphImages(IList<NamedArchiveEntry> files)
        {
            var result = new List<ImageData>();

            IEnumerable<NamedArchiveEntry> imageFiles = files.Where(f => f.Name.EndsWith(".xi"));
            foreach (NamedArchiveEntry imageFile in imageFiles)
            {
                ImageData glyphImage = ImageParser.Parse(imageFile.Content);

                result.Add(glyphImage);
            }

            return result.ToArray();
        }

        private static PlatformType GetPlatform(FontData fontData, ImageData[] glyphImages)
        {
            if (glyphImages.Any(i => i.Version.Platform != fontData.Version.Platform))
                throw new InvalidOperationException("Inconsistent platform indicators in font file.");

            return fontData.Version.Platform;
        }
    }
}
