using System;
using System.Drawing;
using CfgBinEditor.Components;
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
            switch (entry)
            {
                case (RdbnTypeDeclaration type, object[][] values):
                    return IsListEntrySearched(type, values, searchText);
            }

            return false;
        }

        private bool IsListEntrySearched(RdbnTypeDeclaration type, object[][] values, string searchText)
        {
            for (var i = 0; i < type.Fields.Length; i++)
            {
                RdbnFieldDeclaration field = type.Fields[i];

                for (var j = 0; j < field.Count; j++)
                {
                    if (IsFieldSearched(values[i][j], field.FieldType, searchText))
                        return true;
                }
            }

            return false;
        }

        private bool IsFieldSearched(object value, FieldType fieldType, string searchText)
        {
            switch (fieldType)
            {
                case FieldType.Bool:
                    if (searchText.Equals("false", StringComparison.OrdinalIgnoreCase))
                        return !(bool)value;

                    if (searchText.Equals("true", StringComparison.OrdinalIgnoreCase))
                        return (bool)value;

                    break;

                case FieldType.RateMatrix:
                case FieldType.StatusRate:
                    var floatValues = (float[])value;
                    foreach (float floatValue in floatValues)
                    {
                        string valueText1 = RdbnValueComponent.GetValueText(floatValue, fieldType);
                        if (valueText1.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                            return true;
                    }

                    break;

                case FieldType.DataTuple:
                    var shortValues = (short[])value;
                    foreach (short shortValue in shortValues)
                    {
                        string valueText1 = RdbnValueComponent.GetValueText(shortValue, fieldType);
                        if (valueText1.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                            return true;
                    }

                    break;

                default:
                    string valueText3 = RdbnValueComponent.GetValueText(value, fieldType);
                    if (valueText3.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                        return true;

                    break;
            }

            return false;
        }
    }
}
