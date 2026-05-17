using plugin_time_travelers.Formats.Font.DataClasses;

namespace plugin_time_travelers.Formats.Font.InternalContract
{
    internal interface IFontReader
    {
        FontData Read(Stream input);
    }
}
