using ImGui.Forms.Controls.Base;

namespace CfgBinEditor.Messages
{
    public class FileChangedMessage
    {
        public Component Source { get; }

        public FileChangedMessage(Component source)
        {
            Source = source;
        }
    }
}
