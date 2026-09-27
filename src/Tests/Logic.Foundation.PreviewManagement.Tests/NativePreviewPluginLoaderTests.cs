using Kaligraphy.Contract.DataClasses.Parsing;
using Konnect.Contract.Management.Plugin;
using Logic.Foundation.PreviewManagement.Abstract;
using Shouldly;
using Xunit;

namespace Logic.Foundation.PreviewManagement.Tests;

public class NativePreviewPluginLoaderTests
{
    [Fact]
    public async Task Load_PublishedLibrary_ReadsMetadataAndCharacters()
    {
        string publishDirectory = Environment.GetEnvironmentVariable("CBE_NATIVE_PLUGIN_DIR")
            ?? Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..", "..", "..", "..",
                "plugins", "plugin_time_travelers", "bin", "Release", "win-x64", "publish"));

        string nativeLibrary = Path.Combine(publishDirectory, "plugin_time_travelers.dll");
        bool requirePublishedLibrary = Environment.GetEnvironmentVariable("CBE_NATIVE_PLUGIN_DIR") is not null;
        if (!File.Exists(nativeLibrary))
        {
            if (requirePublishedLibrary)
                throw new FileNotFoundException("Published native preview plugin was not found.", nativeLibrary);

            return;
        }

        IPluginManager plugins = PreviewPluginHost.Create(publishDirectory);
        IPreviewPlugin narration = plugins.GetPlugins<IPreviewPlugin>().Single(plugin => plugin.Metadata.Name == "Time Travelers Narration");
        plugins.GetPlugins<IPreviewPlugin>().ShouldContain(plugin => plugin.Metadata.Name == "Time Travelers Subtitle");

        IList<Kaligraphy.Contract.DataClasses.Parsing.CharacterData> characters =
            narration.Deserializer!.Deserialize("<TIP001>[あ/い]");
        characters.Count.ShouldBeGreaterThan(1);
        characters.ShouldContain(character => character.IsVisible == false);

        byte[] rewritten = CharacterWireEnvelope.Write(characters.Select(character =>
        {
            WireCharacterData wire = character.ShouldBeOfType<WireCharacterData>();
            CharacterData edited = wire.WithFlags(wire.IsVisible, wire.IsPersistent);
            return edited;
        }).ToList());
        CharacterWireEnvelope.Read(rewritten).Count.ShouldBe(characters.Count);

        SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>? image =
            await narration.RenderPreview(characters);
        if (image is not null)
        {
            image.Width.ShouldBeGreaterThan(0);
            image.Height.ShouldBeGreaterThan(0);
        }
    }
}
