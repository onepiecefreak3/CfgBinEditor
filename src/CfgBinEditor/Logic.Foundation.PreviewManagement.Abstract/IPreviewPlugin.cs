using Kaligraphy.Contract.DataClasses.Parsing;
using Kaligraphy.Contract.Parsing;
using Konnect.Contract.Plugin;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Logic.Foundation.PreviewManagement.Abstract
{
	public interface IPreviewPlugin : IPlugin
	{
		ICharacterParser? Parser { get; }
		ICharacterComposer? Composer { get; }
		ICharacterSerializer? Serializer { get; }
		ICharacterDeserializer? Deserializer { get; }

		Task<Image<Rgba32>?> RenderPreview(IList<CharacterData> characters);
	}
}
