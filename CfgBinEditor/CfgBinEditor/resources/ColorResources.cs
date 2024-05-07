using ImGui.Forms;
using System.Drawing;
using ImGuiNET;

namespace CfgBinEditor.resources
{
    internal class ColorResources
    {
        public static ThemedColor TextDefault => ImGuiCol.Text;

        public static ThemedColor TextSuccessful => new(Color.ForestGreen, Color.FromArgb(0x49, 0xe7, 0x9a));

        public static ThemedColor TextError => new(Color.DarkRed, Color.FromArgb(0xcf, 0x66, 0x79));
    }
}
