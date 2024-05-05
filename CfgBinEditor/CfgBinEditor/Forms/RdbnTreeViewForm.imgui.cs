using System;
using System.Drawing;
using CfgBinEditor.resources;
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
                TreeNode<object> rootNode = CreateListNode(config, root, ColorResources.TextDefault);
                treeView.Nodes.Add(rootNode);
            }
        }

        private TreeNode<object> CreateListNode(Rdbn config, RdbnListEntry entry, Color nodeColor)
        {
            var rootNode = new TreeNode<object> { Text = entry.Name, Data = entry, IsExpanded = true, TextColor = nodeColor };

            RdbnTypeDeclaration type = config.Types[entry.TypeIndex];

            for (var i = 0; i < entry.Values.Length; i++)
            {
                TreeNode<object> typeNode = CreateValueNode(type, entry.Values[i], i + 1, nodeColor);
                rootNode.Nodes.Add(typeNode);
            }

            return rootNode;
        }

        private TreeNode<object> CreateValueNode(RdbnTypeDeclaration type, object[][] values, int index, Color nodeColor)
        {
            return new TreeNode<object> { Text = GetNodeName(type, index), Data = (type, values), TextColor = nodeColor };
        }

        private string GetNodeName(RdbnTypeDeclaration type, int index)
        {
            return type.Name + $"_{index}";
        }

        protected override bool IsEntrySearched(object entry, string searchText)
        {
            throw new NotImplementedException();
        }
    }
}
