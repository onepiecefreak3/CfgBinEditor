using CfgBinEditor.InternalContract.DataClasses;
using ImGui.Forms.Localization;

namespace CfgBinEditor.Messages
{
    public class UpdateStatusMessage
    {
        public LocalizedString Text { get; }
        public LabelStatus Status { get; }

        public UpdateStatusMessage(LocalizedString text, LabelStatus status)
        {
            Text = text;
            Status = status;
        }
    }
}
