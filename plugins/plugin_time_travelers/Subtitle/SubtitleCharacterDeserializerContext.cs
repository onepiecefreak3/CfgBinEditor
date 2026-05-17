using plugin_time_travelers.Rendering.Deserialization;

namespace plugin_time_travelers.Subtitle
{
    class SubtitleCharacterDeserializerContext : TimeTravelersDeserializerContext
    {
        public bool IsSubtitle { get; set; }
    }
}
