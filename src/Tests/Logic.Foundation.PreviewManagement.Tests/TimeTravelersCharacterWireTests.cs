extern alias timetravelers;

using Kaligraphy.Contract.DataClasses.Parsing;
using Shouldly;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using timetravelers::Logic.Foundation.PreviewManagement.Generated;
using timetravelers::plugin_time_travelers.Narration;
using timetravelers::plugin_time_travelers.Rendering.Deserialization.CharacterData;
using Xunit;

namespace Logic.Foundation.PreviewManagement.Tests;

public class TimeTravelersCharacterWireTests
{
    [Fact]
    public async Task Write_AllConcreteCharacters_RoundTripsTypeAndProperties()
    {
        CharacterData[] characters =
        [
            new FontCharacterData { IsVisible = true, IsPersistent = true, Character = 'A' },
            new LineBreakCharacterData { IsVisible = false, IsPersistent = false, LineBreak = "\r\n" },
            new FuriganaStartCharacterData { IsVisible = false, IsPersistent = false, Character = '[' },
            new FuriganaSplitCharacterData { IsVisible = false, IsPersistent = true, Character = '/' },
            new FuriganaEndCharacterData { IsVisible = true, IsPersistent = false, Character = ']' },
            new TipStartControlCodeCharacterData { IsVisible = false, IsPersistent = false, TipNumber = 12 },
            new TipEndControlCodeCharacterData { IsVisible = false, IsPersistent = false },
            new IconControlCodeCharacterData { IsVisible = true, IsPersistent = true, IconName = "star" },
            new BlankControlCodeCharacterData { IsVisible = false, IsPersistent = true, Width = 24 },
            new GenericControlCodeCharacterData { IsVisible = false, IsPersistent = false, Code = "WAIT" }
        ];

        byte[] bytes = PreviewCharacterWire.Write(characters);
        List<CharacterData> read = PreviewCharacterWire.Read(bytes);

        read.Count.ShouldBe(characters.Length);
        for (int index = 0; index < characters.Length; index++)
        {
            read[index].GetType().ShouldBe(characters[index].GetType());
            read[index].IsVisible.ShouldBe(characters[index].IsVisible);
            read[index].IsPersistent.ShouldBe(characters[index].IsPersistent);
        }

        ((FontCharacterData)read[0]).Character.ShouldBe((ushort)'A');
        ((LineBreakCharacterData)read[1]).LineBreak.ShouldBe("\r\n");
        ((FuriganaStartCharacterData)read[2]).Character.ShouldBe((ushort)'[');
        ((TipStartControlCodeCharacterData)read[5]).TipNumber.ShouldBe(12);
        ((IconControlCodeCharacterData)read[7]).IconName.ShouldBe("star");
        ((BlankControlCodeCharacterData)read[8]).Width.ShouldBe(24);
        ((GenericControlCodeCharacterData)read[9]).Code.ShouldBe("WAIT");

        var plugin = new NarrationPreviewPlugin();
        Image<Rgba32>? image = await plugin.RenderPreview(read);
        if (image is not null)
            image.Width.ShouldBeGreaterThan(0);
    }
}
