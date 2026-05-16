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
