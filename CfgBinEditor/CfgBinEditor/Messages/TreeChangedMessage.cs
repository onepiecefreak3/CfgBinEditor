using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CfgBinEditor.Forms;

namespace CfgBinEditor.Messages
{
    internal class TreeChangedMessage<TConfig, TEntry> where TEntry : class
    {
        public BaseTreeViewForm<TConfig, TEntry> TreeViewForm { get; }

        public TreeChangedMessage(BaseTreeViewForm<TConfig, TEntry> treeViewForm)
        {
            TreeViewForm = treeViewForm;
        }
    }
}
