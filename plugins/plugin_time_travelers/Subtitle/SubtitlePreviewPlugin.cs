using Kaligraphy.Contract.Layout;
using Kaligraphy.Contract.Parsing;
using Kaligraphy.Contract.Rendering;
using Kaligraphy.Enums.Layout;
using Konnect.Contract.DataClasses.Plugin;
using Logic.Foundation.PreviewManagement.Abstract;
using plugin_time_travelers.Formats.Font;
using plugin_time_travelers.Formats.Font.DataClasses;
using plugin_time_travelers.Rendering;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace plugin_time_travelers.Subtitle
{
    public class SubtitlePreviewPlugin : IPreviewPlugin
    {
        private FontImageData? _font;

        public Guid PluginId => Guid.Parse("4d3abddf-70d9-4ec5-a5a6-bc102ffe3bc0");

        public PluginMetadata Metadata => new()
        {
            Author = ["onepiecefreak"],
            Name = "Time Travelers Subtitle",
            Publisher = "Level5",
            Developer = "Level5",
            Platform = ["3DS", "Vita", "Psp"],
            LongDescription = "Preview plugin for Time Travelers."
        };

        public ICharacterDeserializer? Deserializer { get; } = new SubtitleCharacterDeserializer();

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

            var initPoint = new Point(0, 15);
            var layout = layouter.Create(characters, initPoint, screen.Size);
            renderer.Render(screen, layout);

            return screen;
        }

        private Image<Rgba32> GetScreen()
        {
            var image = new Image<Rgba32>(400, 240);
            image.Mutate(x => x.Clear(Color.Wheat));

            return image;
        }

        private ITextLayouter GetLayouter(IGlyphProvider glyphProvider, IGlyphProvider furiganaProvider)
        {
            return new FuriganaTextLayouter(new FuriganaLayoutOptions
            {
                HorizontalAlignment = HorizontalTextAlignment.Center,
                VerticalAlignment = VerticalTextAlignment.Bottom,
                LineHeight = 21,
                LineWidth = 286
            }, glyphProvider, furiganaProvider);
        }

        private ITextRenderer GetRenderer(IGlyphProvider glyphProvider, IGlyphProvider furiganaProvider)
        {
            return new FuriganaTextRenderer(new FuriganaRenderOptions
            {
                VisibleLines = 2,
                OutlineRadius = 3,
                TextColor = Color.FromRgb(0xCE, 0xCE, 0xCE),
                TextOutlineColor = Color.Black
            }, glyphProvider, furiganaProvider);
        }

        private FontImageData? GetFont()
        {
            if (_font is not null)
                return _font;

            string resourceDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins", "time_travelers");
            if (!Directory.Exists(resourceDirectory))
                return null;

            string resourcePath = Path.Combine(resourceDirectory, "nrm_main.xf");
            if (!File.Exists(resourceDirectory))
                return null;

            return _font = FontParser.Parse(File.OpenRead(resourcePath));
        }
    }
}
