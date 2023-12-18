using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CfgBinEditor.Forms;

namespace CfgBinEditor.Messages
{
    internal class ConfigurationTreeChangedMessage
    {
        public ConfigurationTreeViewForm TreeViewForm { get; }

        public ConfigurationTreeChangedMessage(ConfigurationTreeViewForm treeViewForm)
        {
            TreeViewForm = treeViewForm;
        }
    }
}
