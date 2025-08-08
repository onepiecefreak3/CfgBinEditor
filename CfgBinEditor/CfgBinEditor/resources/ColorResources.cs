using ImGui.Forms.Models;
using ImGui.Forms;
using System.Collections.Generic;
using ImGuiNET;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CfgBinEditor.resources
{
    internal class ColorResources
    {
        private const int ImGuiColMax_ = 55;

        private static readonly IDictionary<Theme, IDictionary<uint, Color>> Store = new Dictionary<Theme, IDictionary<uint, Color>>
        {
            [Theme.Dark] = new Dictionary<uint, Color>
            {
                [ImGuiColMax_ + 1] = new Rgba32(0x49, 0xe7, 0x9a),
                [ImGuiColMax_ + 2] = new Rgba32(0xcf, 0x66, 0x79),
            },
            [Theme.Light] = new Dictionary<uint, Color>
            {
                [ImGuiColMax_ + 1] = Color.ForestGreen,
                [ImGuiColMax_ + 2] = Color.DarkRed,
            }
        };

        public static Color TextDefault => Style.GetColor(ImGuiCol.Text);

        public static Color TextSuccessful => Store[Style.Theme][ImGuiColMax_ + 1];

        public static Color TextError => Store[Style.Theme][ImGuiColMax_ + 2];
    }
}
