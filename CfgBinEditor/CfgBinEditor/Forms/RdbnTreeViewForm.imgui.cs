using System;
using ImGui.Forms.Controls.Tree;
using Logic.Domain.Level5Management.Contract.DataClasses;

namespace CfgBinEditor.Forms
{
    public partial class RdbnTreeViewForm
    {
        protected override void PopulateFullTreeView(Rdbn config, TreeView<object> treeView)
        {
            foreach (RdbnListEntry root in config.Lists)
            {
                var rootNode = new TreeNode<object> { Text = root.Name, Data = root, IsExpanded = true };
                treeView.Nodes.Add(rootNode);

                RdbnTypeDeclaration type = config.Types[root.TypeIndex];

                for (var i = 0; i < type.Fields.Length; i++)
                {
                    var typeNode = new TreeNode<object> { Text = type.Name + $"_{i + 1}", Data = (type, root.Values[i]) };
                    rootNode.Nodes.Add(typeNode);
                }
            }
        }

        protected override bool IsEntrySearched(object entry, string searchText)
        {
            throw new NotImplementedException();
        }
    }
}
