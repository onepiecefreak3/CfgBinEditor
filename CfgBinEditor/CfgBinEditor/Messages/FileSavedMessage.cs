using CfgBinEditor.Forms;
using System;
using ImGui.Forms.Controls.Base;

namespace CfgBinEditor.Messages
{
    internal class FileSavedMessage
    {
        public Component Source { get; }
        public Exception Error { get; }

        public FileSavedMessage(Component source, Exception e)
        {
            Source = source;
            Error = e;
        }
    }
}
