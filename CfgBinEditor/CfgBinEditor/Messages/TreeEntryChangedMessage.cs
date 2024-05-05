using CfgBinEditor.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CfgBinEditor.Messages
{
    internal class TreeEntryChangedMessage<TConfig, TEntry> where TEntry:class
    {
        public BaseTreeViewForm<TConfig, TEntry> TreeViewForm { get; }
        public TEntry? Entry { get; }

        public TreeEntryChangedMessage(BaseTreeViewForm<TConfig, TEntry> treeViewForm, TEntry? entry)
        {
            TreeViewForm = treeViewForm;
            Entry = entry;
        }
    }
}
