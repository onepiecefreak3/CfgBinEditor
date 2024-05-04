using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CfgBinEditor.Forms;
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
