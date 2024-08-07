﻿using System;
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

        private const string ClosePath_ = "resources/images/close.png";

        private const string RandomLightPath_ = "resources/images/random_light.png";
        private const string RandomDarkPath_ = "resources/images/random_dark.png";

        public static Image<Rgba32> Icon => FromFile(IconPath_);

        public static ThemedImageResource Save => ImageResource.FromFile(SavePath_);
        public static ThemedImageResource SaveAll => ImageResource.FromFile(SaveAllPath_);

        public static ThemedImageResource Close => ImageResource.FromFile(ClosePath_);

        public static ThemedImageResource Random => new(ImageResource.FromFile(RandomLightPath_), ImageResource.FromFile(RandomDarkPath_));

        private static Image<Rgba32> FromFile(string filePath)
        {
            string fullPath = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath)!, filePath);
            return Image.Load<Rgba32>(fullPath);
        }
    }
}