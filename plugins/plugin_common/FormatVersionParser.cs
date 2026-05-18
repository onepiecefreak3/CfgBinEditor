using plugin_common.Font.DataClasses;
using plugin_common.Font.Enums;

namespace plugin_common
{
    internal class FormatVersionParser
    {
        public static FormatVersion Parse(string magic)
        {
            return new FormatVersion
            {
                Platform = GetPlatform(magic),
                Version = GetVersion(magic)
            };
        }

        private static PlatformType GetPlatform(string magic)
        {
            return magic[3] switch
            {
                'C' => PlatformType.Ctr,
                'P' => PlatformType.Psp,
                'V' => PlatformType.PsVita,
                'A' => PlatformType.Android,
                'N' => PlatformType.Switch,
                _ => throw new InvalidOperationException($"Unknown platform identifier '{magic[3]}' in font.")
            };
        }

        private static int GetVersion(string magic)
        {
            return int.Parse(magic[4..6]);
        }
    }
}
