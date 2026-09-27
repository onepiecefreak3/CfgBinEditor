extern alias yokai;

using Kaligraphy.Contract.DataClasses.Parsing;
using Shouldly;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;
using yokai::Logic.Foundation.PreviewManagement.Generated;
using yokai::plugin_yokai_watch;
using yokai::plugin_yokai_watch.Rendering.Deserialization.CharacterData;

namespace Logic.Foundation.PreviewManagement.Tests;

public class YoKaiWatchCharacterWireTests
{
    [Fact]
    public async Task Write_ColorAndFuriganaCharacters_RoundTripsTypeAndProperties()
    {
        var color = new Rgba32(1, 2, 3, 4);
        CharacterData[] characters =
        [
            new FuriganaStartCharacterData { IsVisible = false, IsPersistent = false, Character = '[' },
            new FuriganaSplitCharacterData { IsVisible = false, IsPersistent = false, Character = '/' },
            new FuriganaEndCharacterData { IsVisible = false, IsPersistent = false, Character = ']' },
            new IconControlCodeCharacterData { IsVisible = true, IsPersistent = true, IconName = "item" },
            new GenericControlCodeCharacterData { IsVisible = false, IsPersistent = false, Code = "PAUSE" },
            new ColorStartControlCodeCharacterData { IsVisible = false, IsPersistent = false, Color = color },
            new ColorEndControlCodeCharacterData { IsVisible = false, IsPersistent = false }
        ];

        List<CharacterData> read = PreviewCharacterWire.Read(PreviewCharacterWire.Write(characters));

        read.Count.ShouldBe(characters.Length);
        for (int index = 0; index < characters.Length; index++)
            read[index].GetType().ShouldBe(characters[index].GetType());

        ((ColorStartControlCodeCharacterData)read[5]).Color.R.ShouldBe(color.R);
        ((ColorStartControlCodeCharacterData)read[5]).Color.G.ShouldBe(color.G);
        ((ColorStartControlCodeCharacterData)read[5]).Color.B.ShouldBe(color.B);
        ((ColorStartControlCodeCharacterData)read[5]).Color.A.ShouldBe(color.A);
        ((IconControlCodeCharacterData)read[3]).IconName.ShouldBe("item");
        ((GenericControlCodeCharacterData)read[4]).Code.ShouldBe("PAUSE");

        var plugin = new DefaultPreviewPlugin();
        SixLabors.ImageSharp.Image<Rgba32>? image = await plugin.RenderPreview(read);
        if (image is not null)
            image.Width.ShouldBeGreaterThan(0);
    }
}
