using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CfgBinEditor.resources
{
    internal static class ImageResources
    {
        private const string IconPath_ = "resources/images/logo.ico";

        public static Image Icon => FromFile(IconPath_);

        private static Image FromFile(string filePath)
        {
            string fullPath = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath)!, filePath);
            return Image.FromFile(fullPath);
        }
    }
}
