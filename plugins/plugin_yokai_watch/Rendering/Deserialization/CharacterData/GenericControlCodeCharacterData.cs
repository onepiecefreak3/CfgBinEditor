using Kaligraphy.Contract.DataClasses.Parsing;

namespace plugin_yokai_watch.Rendering.Deserialization.CharacterData
{
    class GenericControlCodeCharacterData : ControlCodeCharacterData
    {
        public required string Code { get; init; }
    }
}
