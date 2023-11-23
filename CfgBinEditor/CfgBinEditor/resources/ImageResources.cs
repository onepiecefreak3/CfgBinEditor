using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGui.Forms.Resources;

namespace CfgBinEditor.resources
{
    internal static class ImageResources
    {
        private const string IconPath_ = "resources/images/logo.ico";

        private const string SavePath_ = "resources/images/save.png";
        private const string SaveAllPath_ = "resources/images/save_all_1.png";

        public static Image Icon => FromFile(IconPath_);

        public static ImageResource Save => ImageResource.FromFile(SavePath_);
        public static ImageResource SaveAll => ImageResource.FromFile(SaveAllPath_);

        private static Image FromFile(string filePath)
        {
            string fullPath = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath)!, filePath);
            return Image.FromFile(fullPath);
        }
    }
}
