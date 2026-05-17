using Kaligraphy.Contract.Rendering;
using Kaligraphy.DataClasses.Layout;
using Kaligraphy.Layout;
using plugin_time_travelers.Rendering.Deserialization.CharacterData;

namespace plugin_time_travelers.Rendering
{
    internal class FuriganaTextLayouter : TextLayouter<FuriganaLayoutContext, FuriganaLayoutOptions>
    {
        private readonly IGlyphProvider _furiganaProvider;

        public FuriganaTextLayouter(FuriganaLayoutOptions options, IGlyphProvider glyphProvider, IGlyphProvider furiganaProvider) : base(options, glyphProvider)
        {
            _furiganaProvider = furiganaProvider;
        }

        protected override void CreateCharacter(Kaligraphy.Contract.DataClasses.Parsing.CharacterData character, FuriganaLayoutContext context)
        {
            switch (character)
            {
                case FuriganaStartCharacterData:
                    context.IsFuriganaTop = false;
                    context.FuriganaBottomIndex = context.Characters.Count;
                    context.FuriganaTopIndex = -1;
                    context.FuriganaX = context.VisibleX;
                    break;

                case FuriganaSplitCharacterData:
                    context.IsFuriganaTop = true;
                    context.FuriganaWidth = context.VisibleX - context.FuriganaX;
                    context.FuriganaTopIndex = context.Characters.Count;
                    context.VisibleX = context.FuriganaX;
                    break;

                case FuriganaEndCharacterData:
                    if (!context.IsFuriganaTop)
                        break;

                    float topWidth = context.VisibleX - context.FuriganaX;
                    float offset = (int)((context.FuriganaWidth - topWidth) / 2);

                    for (int i = context.FuriganaTopIndex; i < context.Characters.Count; i++)
                    {
                        context.Characters[i].BoundingBox = context.Characters[i].BoundingBox with
                        {
                            X = context.Characters[i].BoundingBox.X + offset
                        };
                        context.Characters[i].GlyphBoundingBox = context.Characters[i].GlyphBoundingBox with
                        {
                            X = context.Characters[i].GlyphBoundingBox.X + offset
                        };
                    }

                    context.IsFuriganaTop = false;
                    context.VisibleX = context.FuriganaX + context.FuriganaWidth;

                    break;
            }

            base.CreateCharacter(character, context);

            if (context.Characters.Count > 0 && !context.IsFuriganaTop)
            {
                context.Characters[^1].BoundingBox = context.Characters[^1].BoundingBox with
                {
                    Y = context.Characters[^1].BoundingBox.Y + _furiganaProvider.GetMaxHeight() + Options.FuriganaLineSpacing
                };
                context.Characters[^1].GlyphBoundingBox = context.Characters[^1].GlyphBoundingBox with
                {
                    Y = context.Characters[^1].GlyphBoundingBox.Y + _furiganaProvider.GetMaxHeight() + Options.FuriganaLineSpacing
                };
            }
        }

        protected override IGlyphProvider GetGlyphProvider(FuriganaLayoutContext context)
        {
            return context.IsFuriganaTop ? _furiganaProvider : base.GetGlyphProvider(context);
        }

        protected override int GetFontHeight()
        {
            return base.GetFontHeight() + _furiganaProvider.GetMaxHeight() + Options.FuriganaLineSpacing;
        }
    }

    class FuriganaLayoutOptions : LayoutOptions
    {
        public int FuriganaLineSpacing { get; set; }
    }

    class FuriganaLayoutContext : LayoutContext
    {
        public bool IsFuriganaTop { get; set; }
        public int FuriganaBottomIndex { get; set; }
        public int FuriganaTopIndex { get; set; }
        public float FuriganaX { get; set; }
        public float FuriganaWidth { get; set; }
    }
}
