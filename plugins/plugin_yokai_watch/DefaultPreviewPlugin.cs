using Kaligraphy.Contract.Layout;
using Kaligraphy.Contract.Parsing;
using Kaligraphy.Contract.Rendering;
using Kaligraphy.Enums.Layout;
using Konnect.Contract.DataClasses.Plugin;
using Logic.Foundation.PreviewManagement.Abstract;
using plugin_common.Font;
using plugin_common.Font.DataClasses;
using plugin_yokai_watch.Rendering;
using plugin_yokai_watch.Rendering.Deserialization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace plugin_yokai_watch
{
    public class DefaultPreviewPlugin : IPreviewPlugin
    {
        private FontImageData? _font;

        public Guid PluginId => Guid.Parse("a27cbceb-ab11-4111-be68-0217be6d9239");

        public PluginMetadata Metadata => new()
        {
            Author = ["onepiecefreak"],
            Name = "Yo-Kai Watch",
            Publisher = "Level5",
            Developer = "Level5",
            Platform = ["3DS", "Vita", "Psp"],
            LongDescription = "Preview plugin for Yo-Kai Watch."
        };

        public ICharacterDeserializer? Deserializer { get; } = new YoKaiWatchCharacterDeserializer<YoKaiWatchDeserializerContext>();

        public async Task<Image<Rgba32>?> RenderPreview(IList<Kaligraphy.Contract.DataClasses.Parsing.CharacterData> characters)
        {
            FontImageData? font = GetFont();
            if (font is null)
                return null;

            var furiganaProvider = GlyphProviderFactory.Create(font, true);
            var glyphProvider = GlyphProviderFactory.Create(font, false);

            var screen = GetScreen();

            var layouter = GetLayouter(glyphProvider, furiganaProvider);
            var renderer = GetRenderer(glyphProvider, furiganaProvider);

            var initPoint = new Point(16, 1);
            var layout = layouter.Create(characters, initPoint, screen.Size);
            renderer.Render(screen, layout);

            return screen;
        }

        private Image<Rgba32> GetScreen()
        {
            var image = new Image<Rgba32>(320, 240);

            return image;
        }

        private ITextLayouter GetLayouter(IGlyphProvider glyphProvider, IGlyphProvider furiganaProvider)
        {
            return new YoKaiWatchTextLayouter(new FuriganaLayoutOptions
            {
                HorizontalAlignment = HorizontalTextAlignment.Left,
                VerticalAlignment = VerticalTextAlignment.Top,
                LineHeight = 25,
                LineWidth = 286
            }, glyphProvider, furiganaProvider);
        }

        private ITextRenderer GetRenderer(IGlyphProvider glyphProvider, IGlyphProvider furiganaProvider)
        {
            return new YoKaiWatchTextRenderer(new FuriganaRenderOptions
            {
                TextColor = Color.FromRgb(0xFD, 0xFD, 0xFD),
                FuriganaTextColor = Color.FromRgb(0xFD, 0xFD, 0xFD)
            }, glyphProvider, furiganaProvider);
        }

        private FontImageData? GetFont()
        {
            if (_font is not null)
                return _font;

            string resourceDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins", "yokai_watch");
            if (!Directory.Exists(resourceDirectory))
                return null;

            string resourcePath = Path.Combine(resourceDirectory, "nrm_main.xf");
            if (!File.Exists(resourcePath))
                return null;

            return _font = FontParser.Parse(File.OpenRead(resourcePath));
        }
    }
}
