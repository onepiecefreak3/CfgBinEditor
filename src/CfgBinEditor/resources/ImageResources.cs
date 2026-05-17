using System;
using System.IO;
using ImGui.Forms.Resources;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CfgBinEditor.resources
{
    internal static class ImageResources
    {
        private const string IconPath_ = "resources/images/logo.png";

        private const string SavePath_ = "resources/images/save.png";
        private const string SaveAllPath_ = "resources/images/save_all_1.png";

        private const string ImageExportPath_ = "resources/images/image_export.png";
        private const string SettingsPath_ = "resources/images/settings.png";

        private const string ClosePath_ = "resources/images/close.png";

        private const string RandomLightPath_ = "resources/images/random_light.png";
        private const string RandomDarkPath_ = "resources/images/random_dark.png";

        public static Image<Rgba32> Icon => Image.Load<Rgba32>(GetFullPath(IconPath_));

        public static ThemedImageResource Save => ImageResource.FromFile(GetFullPath(SavePath_));
        public static ThemedImageResource SaveAll => ImageResource.FromFile(GetFullPath(SaveAllPath_));

        public static ThemedImageResource ImageExport => ImageResource.FromFile(GetFullPath(ImageExportPath_));
        public static ThemedImageResource Settings => ImageResource.FromFile(GetFullPath(SettingsPath_));

        public static ThemedImageResource Close => ImageResource.FromFile(GetFullPath(ClosePath_));

        public static ThemedImageResource Random => new(ImageResource.FromFile(GetFullPath(RandomLightPath_)),
            ImageResource.FromFile(GetFullPath(RandomDarkPath_)));

        private static string GetFullPath(string relativePath) => Path.Combine(Path.GetDirectoryName(Environment.ProcessPath)!, relativePath);
    }
}