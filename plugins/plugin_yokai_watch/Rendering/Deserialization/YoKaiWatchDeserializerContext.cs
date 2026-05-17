using Kaligraphy.DataClasses.Parsing;

namespace plugin_yokai_watch.Rendering.Deserialization
{
    class YoKaiWatchDeserializerContext : CharacterDeserializerContext
    {
        public bool IsFuriganaBottom { get; set; }
        public bool IsFuriganaTop { get; set; }
    }
}
