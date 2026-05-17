using Kaligraphy.DataClasses.Parsing;

namespace plugin_time_travelers.Rendering.Deserialization
{
    class TimeTravelersDeserializerContext : CharacterDeserializerContext
    {
        public bool IsFuriganaBottom { get; set; }
        public bool IsFuriganaTop { get; set; }
    }
}
