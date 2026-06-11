using System.Globalization;
using Kaligraphy.Contract.DataClasses.Parsing;
using Kaligraphy.Parsing;
using plugin_yokai_watch.Rendering.Deserialization.CharacterData;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace plugin_yokai_watch.Rendering.Deserialization
{
    class YoKaiWatchCharacterDeserializer<TContext> : CharacterDeserializer<TContext>
        where TContext : YoKaiWatchDeserializerContext, new()
    {
        private readonly Rgba32[] _colors =
        [
            new(0x00, 0xFF, 0x00), new(0x00, 0x00, 0x00), new(0xFF, 0xFF, 0xFF), new(0xC8, 0x00, 0x14),
            new(0xC8, 0xA0, 0x14), new(0x32, 0x82, 0x28), new(0x28, 0x50, 0xB4), new(0xD7, 0xB4, 0x5F),
            new(0xE1, 0x82, 0x00), new(0xA5, 0x64, 0x41), new(0x80, 0x80, 0x80), new(0xA0, 0xA0, 0xA0),
            new(0xFF, 0x6E, 0x6E), new(0x82, 0xAA, 0xFA), new(0xD7, 0xFA, 0x64), new(0x00, 0x00, 0x00),
            new(0x32, 0x32, 0x32), new(0xFE, 0xD2, 0x00), new(0x8C, 0xFA, 0x41), new(0xFF, 0x46, 0x46)
        ];

        protected override bool TryDeserializeControlCode(TContext context, int position, out int length,
            out ControlCodeCharacterData? controlCode)
        {
            if (IsIcon(context, position, out length, out string iconName))
            {
                controlCode = new IconControlCodeCharacterData { IsVisible = true, IsPersistent = true, IconName = iconName };
                return true;
            }

            if (IsColorStart(context, position, out length, out Rgba32 color))
            {
                controlCode = new ColorStartControlCodeCharacterData { IsVisible = false, IsPersistent = false, Color = color };
                return true;
            }

            if (IsColorEnd(context, position, out length))
            {
                controlCode = new ColorEndControlCodeCharacterData { IsVisible = false, IsPersistent = false };
                return true;
            }

            if (IsControlCode(context, position, out length, out string controlCodeText))
            {
                controlCode = new GenericControlCodeCharacterData { IsVisible = false, IsPersistent = false, Code = controlCodeText };
                return true;
            }

            controlCode = null;
            return false;
        }

        protected override bool TryDeserializeCharacter(TContext context, int position, out int length,
            out TextCharacterData? textCharacter)
        {
            length = 0;
            textCharacter = null;

            if (context.Text is null)
                return false;

            if (IsLineBreak(context, position, out length, out string lineBreak))
            {
                textCharacter = new LineBreakCharacterData { IsVisible = false, LineBreak = lineBreak };
                return true;
            }

            length = 1;

            if (context.Text[position] is '[')
            {
                context.IsFuriganaBottom = true;
                context.IsFuriganaTop = false;

                textCharacter = new FuriganaStartCharacterData { IsVisible = false, IsPersistent = false, Character = context.Text[position] };
                return true;
            }

            if (context.Text[position] is '/' && context.IsFuriganaBottom)
            {
                context.IsFuriganaBottom = false;
                context.IsFuriganaTop = true;

                textCharacter = new FuriganaSplitCharacterData { IsVisible = false, IsPersistent = false, Character = context.Text[position] };
                return true;
            }

            if (context.Text[position] is ']')
            {
                context.IsFuriganaBottom = false;
                context.IsFuriganaTop = false;

                textCharacter = new FuriganaEndCharacterData { IsVisible = false, IsPersistent = false, Character = context.Text[position] };
                return true;
            }

            textCharacter = new FontCharacterData { IsVisible = true, IsPersistent = true, Character = context.Text[position] };
            return true;
        }

        protected override bool IsLineBreak(TContext context, int position, out int length, out string lineBreak)
        {
            length = 1;
            lineBreak = string.Empty;

            if (context.Text is null)
                return false;

            if (context.Text[position] == '\n')
            {
                lineBreak = "\n";
                return true;
            }

            if (position + 1 >= context.Text.Length)
                return false;

            length = 2;
            bool isLineBreak = context.Text[position] == '\r' && context.Text[position + 1] == '\n'
                               || context.Text[position] == '\\' && context.Text[position + 1] == 'n';

            if (!isLineBreak)
                return false;

            lineBreak = context.Text[position..(position + 2)];
            return true;
        }

        private bool IsIcon(YoKaiWatchDeserializerContext context, int position, out int length, out string iconName)
        {
            length = 2;
            iconName = string.Empty;

            if (context.Text is null)
                return false;

            if (context.Text[position] != '[')
                return false;

            int endIndex = context.Text.IndexOf(']', position);
            if (endIndex < 0)
                return false;

            if (context.Text.IndexOf('[', position + 1, endIndex - position - 1) >= 0)
                return false;

            if (context.Text.IndexOf('/', position + 1, endIndex - position - 1) >= 0)
                return false;

            length = endIndex - position + 1;
            iconName = context.Text[(position + 1)..endIndex];

            return true;
        }

        private bool IsColorStart(YoKaiWatchDeserializerContext context, int position, out int length, out Rgba32 color)
        {
            length = 2;
            color = Color.Transparent;

            if (context.Text is null)
                return false;

            if (position + length >= context.Text.Length)
                return false;

            if (context.Text[position..(position + length)] != "<C")
                return false;

            length = 4;

            if (position + length >= context.Text.Length)
                return false;

            if (context.Text[position..(position + length)] == "<CR>")
            {
                color = new Rgba32(0xC8, 0x00, 0x14);
                return true;
            }

            if (context.Text[position..(position + length)] == "<CG>")
            {
                color = new Rgba32(0x32, 0x82, 0x28);
                return true;
            }

            if (context.Text[position..(position + length)] == "<CN>")
            {
                color = new Rgba32(0x28, 0x50, 0xB4);
                return true;
            }

            position += 2;

            if (context.Text[position] is '"' or '#')
            {
                position++;

                int endIndex = context.Text.IndexOf('>', position);
                if (endIndex < 0)
                    return false;

                var rgbLength = endIndex - position;
                if (rgbLength > 6)
                    return false;

                length = 4 + rgbLength;

                var colorString = context.Text[position..endIndex];
                for (var i = rgbLength; i < 6; i++)
                    colorString += i % 2 == 0 ? '0' : 'C';
                colorString += "00";

                if (!uint.TryParse(colorString, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint packedValue))
                    return false;

                color = new Rgba32(packedValue);
                return true;
            }

            if (context.Text[position] is >= '0' and <= '9')
            {
                int index;
                if (context.Text[position + 1] is >= '0' and <= '9')
                {
                    length = 5;

                    if (context.Text[position + 2] != '>')
                        return false;

                    index = int.Parse(context.Text[position..(position + 2)]);
                    if (_colors.Length <= index)
                        return false;

                    color = _colors[index];
                    return true;
                }

                length = 4;

                if (context.Text[position + 2] != '>')
                    return false;

                index = context.Text[position] - '0';
                if (_colors.Length <= index)
                    return false;

                color = _colors[index];
                return true;
            }

            return false;
        }

        private bool IsColorEnd(YoKaiWatchDeserializerContext context, int position, out int length)
        {
            length = 4;

            if (context.Text is null)
                return false;

            if (position + length >= context.Text.Length)
                return false;

            if (context.Text[position..(position + length)] != "</C>")
                return false;

            return true;
        }

        private bool IsControlCode(YoKaiWatchDeserializerContext context, int position, out int length, out string controlCodeText)
        {
            length = 2;
            controlCodeText = string.Empty;

            if (context.Text is null)
                return false;

            if (context.Text[position] != '<')
                return false;

            int endIndex = context.Text.IndexOf('>', position);
            if (endIndex < 0)
                return false;

            if (context.Text.IndexOf('<', position + 1, endIndex - position - 1) >= 0)
                return false;

            length = endIndex - position + 1;
            controlCodeText = context.Text[(position + 1)..endIndex];

            return true;
        }
    }
}
