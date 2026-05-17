using Kaligraphy.DataClasses.Parsing;
using plugin_time_travelers.Rendering.Deserialization;
using plugin_time_travelers.Rendering.Deserialization.CharacterData;

namespace plugin_time_travelers.Narration
{
    class NarrationCharacterDeserializer : TimeTravelersCharacterDeserializer<TimeTravelersDeserializerContext>
    {
        protected override bool TryDeserializeCharacter(TimeTravelersDeserializerContext context, int position, out int length,
            out TextCharacterData? textCharacter)
        {
            bool isValid = base.TryDeserializeCharacter(context, position, out length, out textCharacter);

            if (!isValid)
                return false;

            switch (textCharacter)
            {
                case FuriganaCharacterData:
                    break;

                case FontCharacterData fontCharacter:
                    textCharacter = new FontCharacterData
                    {
                        IsVisible = fontCharacter.Character is not '＊' && fontCharacter.IsVisible,
                        IsPersistent = fontCharacter.Character is not '＊' && fontCharacter.IsPersistent,
                        Character = fontCharacter.Character
                    };
                    break;
            }

            return true;
        }
    }
}
