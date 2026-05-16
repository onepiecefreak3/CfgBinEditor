using CfgBinEditor.Forms;

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
