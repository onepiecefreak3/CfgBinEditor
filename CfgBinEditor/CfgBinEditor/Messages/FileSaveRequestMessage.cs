using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CfgBinEditor.Forms;
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
