using plugin_common.Font.DataClasses;

namespace plugin_common.Font.InternalContract
{
    internal interface IFontReader
    {
        FontData Read(Stream input);
    }
}
