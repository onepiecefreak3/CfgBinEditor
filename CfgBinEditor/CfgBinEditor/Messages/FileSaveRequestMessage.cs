using System.Collections.Generic;
using ImGui.Forms.Controls.Base;

namespace CfgBinEditor.Messages
{
    internal class FileSaveRequestMessage
    {
        public IDictionary<Component, string> ConfigForms { get; }

        public FileSaveRequestMessage(IDictionary<Component, string> configForms)
        {
            ConfigForms = configForms;
        }
    }
}
