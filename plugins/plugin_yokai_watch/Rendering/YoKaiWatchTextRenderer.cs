using Kaligraphy.Contract.DataClasses.Layout;
using Kaligraphy.Contract.Rendering;
using Kaligraphy.DataClasses.Rendering;
using Kaligraphy.Rendering;
using plugin_yokai_watch.Rendering.Deserialization.CharacterData;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace plugin_yokai_watch.Rendering
{
    internal class YoKaiWatchTextRenderer : TextRenderer<FuriganaRenderContext, FuriganaRenderOptions>
    {
        private readonly IGlyphProvider _furiganaProvider;

        public YoKaiWatchTextRenderer(FuriganaRenderOptions options, IGlyphProvider glyphProvider, IGlyphProvider furiganaProvider) : base(options, glyphProvider)
        {
            _furiganaProvider = furiganaProvider;
        }

        protected override void RenderCharacter(Image<Rgba32> image, TextLayoutCharacterData character, FuriganaRenderContext context, bool isLineVisible)
        {
            switch (character.Character)
            {
                case FuriganaStartCharacterData:
                    context.IsFuriganaTop = false;
                    break;

                case FuriganaSplitCharacterData:
                    context.IsFuriganaTop = true;
                    break;

                case FuriganaEndCharacterData:
                    context.IsFuriganaTop = false;
                    break;

                case ColorStartControlCodeCharacterData cs:
                    context.OverrideColor = cs.Color;
                    break;

                case ColorEndControlCodeCharacterData:
                    context.OverrideColor = null;
                    break;
            }

            base.RenderCharacter(image, character, context, isLineVisible);
        }

        protected override Color GetTextColor(FuriganaRenderContext context)
        {
            if (context.OverrideColor.HasValue)
                return context.OverrideColor.Value;

            return context.IsFuriganaTop ? Options.FuriganaTextColor : base.GetTextColor(context);
        }

        protected override IGlyphProvider GetGlyphProvider(FuriganaRenderContext context)
        {
            return context.IsFuriganaTop ? _furiganaProvider : base.GetGlyphProvider(context);
        }
    }

    class FuriganaRenderOptions : RenderOptions
    {
        public Color FuriganaTextColor { get; set; }
    }

    class FuriganaRenderContext : RenderContext
    {
        public bool IsFuriganaTop { get; set; }
        public Rgba32? OverrideColor { get; set; }
    }
}
