using Kaligraphy.Contract.Layout;
using Kaligraphy.Contract.Parsing;
using Kaligraphy.Contract.Rendering;
using Kaligraphy.Enums.Layout;
using Konnect.Contract.DataClasses.Plugin;
using Logic.Foundation.PreviewManagement.Abstract;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Reflection;
using plugin_common.Font;
using plugin_common.Font.DataClasses;
using plugin_time_travelers.Rendering;

namespace plugin_time_travelers.Narration
{
    public class NarrationPreviewPlugin : IPreviewPlugin
    {
        private FontImageData? _font;

        public Guid PluginId => Guid.Parse("a21a4442-ead0-4707-9b3d-caf7806e3a47");

        public PluginMetadata Metadata => new()
        {
            Author = ["onepiecefreak"],
            Name = "Time Travelers Narration",
            Publisher = "Level5",
            Developer = "Level5",
            Platform = ["3DS", "Vita", "Psp"],
            LongDescription = "Preview plugin for Time Travelers."
        };

        public ICharacterDeserializer? Deserializer { get; } = new NarrationCharacterDeserializer();

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
            image.Mutate(x => x.Clear(Color.Black));

            Image<Rgba32>? narrationImage = GetNarrationResource();
            if (narrationImage == null)
                return image;

            var narrationPoint = new Point(0, image.Height - narrationImage.Height);
            image.Mutate(x => x.DrawImage(narrationImage, narrationPoint, 1f));

            return image;
        }

        private ITextLayouter GetLayouter(IGlyphProvider glyphProvider, IGlyphProvider furiganaProvider)
        {
            return new FuriganaTextLayouter(new FuriganaLayoutOptions
            {
                HorizontalAlignment = HorizontalTextAlignment.Left,
                VerticalAlignment = VerticalTextAlignment.Top,
                LineHeight = 25,
                LineWidth = 286
            }, glyphProvider, furiganaProvider);
        }

        private ITextRenderer GetRenderer(IGlyphProvider glyphProvider, IGlyphProvider furiganaProvider)
        {
            return new FuriganaTextRenderer(new FuriganaRenderOptions
            {
                TextColor = Color.FromRgb(0xFD, 0xFD, 0xFD),
                FuriganaTextColor = Color.FromRgb(0xFD, 0xFD, 0xFD)
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
            if (!File.Exists(resourcePath))
                return null;

            return _font = FontParser.Parse(File.OpenRead(resourcePath));
        }

        private Image<Rgba32>? GetNarrationResource()
        {
            Stream? boxStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("narration.png");
            if (boxStream is null)
                return null;

            return Image.Load<Rgba32>(boxStream);
        }
    }
}
