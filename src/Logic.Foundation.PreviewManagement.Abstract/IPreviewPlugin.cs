using Kaligraphy.Contract.DataClasses.Parsing;
using Kaligraphy.Contract.Parsing;
using Konnect.Contract.Plugin;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Logic.Foundation.PreviewManagement.Abstract
{
	public interface IPreviewPlugin : IPlugin
	{
		ICharacterDeserializer? Deserializer { get; }

		Task<Image<Rgba32>?> RenderPreview(IList<CharacterData> characters);
	}
}
