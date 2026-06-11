using Kaligraphy.Contract.DataClasses.Parsing;

namespace plugin_time_travelers.Rendering.Deserialization.CharacterData
{
    class TipStartControlCodeCharacterData : ControlCodeCharacterData
    {
        public required int TipNumber { get; init; }
    }
}
