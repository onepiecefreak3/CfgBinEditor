using CfgBinEditor.resources;
using ImGui.Forms.Controls.Layouts;
using ImGui.Forms.Controls.Menu;
using ImGui.Forms.Controls.Tree;
using ImGui.Forms.Controls;
using Logic.Domain.Level5.Contract.DataClasses;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Drawing;
using Size = ImGui.Forms.Models.Size;
using Logic.Business.CfgBinValueSettingsManagement.Contract.DataClasses;
using ValueType = Logic.Domain.Level5.Contract.DataClasses.ValueType;

namespace CfgBinEditor.Forms
{
    public partial class ConfigurationTreeViewForm
    {
        private TextBox _searchTextBox;
        private ImageButton _clearSearchButton;

        private StackLayout _searchLayout;
        private StackLayout _mainLayout;

        private TreeView<ConfigurationEntry> _fullTreeView;
        private TreeView<ConfigurationEntry> _filteredTreeView;

        private ContextMenu _treeViewContextMenu;
        private MenuBarButton _duplicateButton;

        private ContextMenu _filteredTreeViewContextMenu;
        private MenuBarButton _filteredDuplicateButton;

        private IDictionary<ConfigurationEntry, TreeNode<ConfigurationEntry>> _entryNodeLookup;

        private void InitializeComponent(Configuration config)
        {
            _searchTextBox = new TextBox { Placeholder = LocalizationResources.CfgBinEntrySearchPlaceholderCaption };
            _clearSearchButton = new ImageButton { Image = ImageResources.Close };

            _fullTreeView = new TreeView<ConfigurationEntry>();
            _filteredTreeView = new TreeView<ConfigurationEntry>();

            _duplicateButton = new MenuBarButton { Text = LocalizationResources.CfgBinEntryDuplicateCaption };
            _treeViewContextMenu = new ContextMenu
            {
                Items =
                {
                    _duplicateButton
                }
            };

            _filteredDuplicateButton = new MenuBarButton { Text = LocalizationResources.CfgBinEntryDuplicateCaption };
            _filteredTreeViewContextMenu = new ContextMenu
            {
                Items =
                {
                    _filteredDuplicateButton
                }
            };

            _fullTreeView.ContextMenu = _treeViewContextMenu;
            _filteredTreeView.ContextMenu = _filteredTreeViewContextMenu;

            _searchLayout = new StackLayout
            {
                Alignment = Alignment.Horizontal,
                ItemSpacing = 5,
                Size = Size.WidthAlign,
                Items =
                {
                    new StackItem(_searchTextBox){VerticalAlignment = VerticalAlignment.Center},
                    _clearSearchButton
                }
            };

            _mainLayout = new StackLayout
            {
                Alignment = Alignment.Vertical,
                ItemSpacing = 5,
                Items =
                {
                    _searchLayout,
                    _fullTreeView
                }
            };

            _entryNodeLookup = new Dictionary<ConfigurationEntry, TreeNode<ConfigurationEntry>>();

            PopulateFullTreeView(config);
        }

        private void ResetNodesChangeState(IList<TreeNode<ConfigurationEntry>> nodes)
        {
            foreach (TreeNode<ConfigurationEntry> node in nodes)
            {
                node.TextColor = Color.Empty;

                if (node.Nodes.Count > 0)
                    ResetNodesChangeState(node.Nodes);
            }
        }

        #region Tree population

        private void PopulateFullTreeView(Configuration config)
        {
            var root = new TreeNode<ConfigurationEntry>();

            _entryNodeLookup.Clear();
            PopulateNode(root, config, 0);

            _fullTreeView.Nodes.Clear();
            foreach (TreeNode<ConfigurationEntry> node in root.Nodes)
                _fullTreeView.Nodes.Add(node);
        }

        private int PopulateNode(TreeNode<ConfigurationEntry> rootNode, Configuration config, int index, string endNodeName = null, Color textColor = default)
        {
            for (; index < config.Entries.Length; index++)
            {
                if (config.Entries[index].Name == endNodeName)
                    break;

                TreeNode<ConfigurationEntry> node = CreateNode(config, ref index, textColor);
                _entryNodeLookup[node.Data] = node;

                rootNode.Nodes.Add(node);
            }

            AdjustNodeNames(rootNode.Nodes);

            return index;
        }

        private void AdjustNodeNames(IList<TreeNode<ConfigurationEntry>> nodes)
        {
            foreach (IGrouping<string, TreeNode<ConfigurationEntry>> group in nodes.GroupBy(x => x.Data.Name, x => x))
            {
                TreeNode<ConfigurationEntry>[] sameNameNodes = group.ToArray();
                if (sameNameNodes.Length <= 1)
                    continue;

                for (var i = 0; i < sameNameNodes.Length; i++)
                    sameNameNodes[i].Text = GetNodeName(sameNameNodes[i].Data) + $"_{i}";
            }
        }

        private TreeNode<ConfigurationEntry> CreateNode(Configuration config, ref int index, Color textColor)
        {
            ConfigurationEntry entry = config.Entries[index];

            var entryNode = new TreeNode<ConfigurationEntry> { Data = entry, Text = entry.Name, TextColor = textColor };

            int beginIndex = GetBeginIndex(entry.Name);
            if (beginIndex < 0)
                return entryNode;

            entryNode.Text = entry.Name[..beginIndex];
            index = PopulateNode(entryNode, config, index + 1, entryNode.Text + "_END", textColor);

            return entryNode;
        }

        private string GetNodeName(ConfigurationEntry entry)
        {
            int beginIndex = GetBeginIndex(entry.Name);
            if (beginIndex < 0)
                return entry.Name;

            return entry.Name[..beginIndex];
        }

        private int GetBeginIndex(string name)
        {
            int beginIndex = name.LastIndexOf("_BEGIN", StringComparison.Ordinal);
            int begIndex = name.LastIndexOf("_BEG", StringComparison.Ordinal);
            int bgnIndex = name.LastIndexOf("_BGN", StringComparison.Ordinal);

            return beginIndex < 0 ? begIndex < 0 ? bgnIndex : begIndex : beginIndex;
        }

        #endregion

        #region Filter nodes

        private IList<TreeNode<ConfigurationEntry>> FilterNodes(IList<TreeNode<ConfigurationEntry>> nodes, string searchText)
        {
            var result = new List<TreeNode<ConfigurationEntry>>();

            foreach (TreeNode<ConfigurationEntry> node in nodes)
            {
                IList<TreeNode<ConfigurationEntry>> filteredChildNodes = FilterNodes(node.Nodes, searchText);
                if (!IsEntrySearched(node.Data, searchText) && filteredChildNodes.Count <= 0)
                    continue;

                var filteredNode = new TreeNode<ConfigurationEntry>
                {
                    Font = node.Font,
                    Text = node.Text,
                    TextColor = node.TextColor,
                    IsExpanded = node.IsExpanded,
                    Data = node.Data,
                };

                foreach (TreeNode<ConfigurationEntry> childNode in filteredChildNodes)
                    filteredNode.Nodes.Add(childNode);

                result.Add(filteredNode);
            }

            return result;
        }

        private bool IsEntrySearched(ConfigurationEntry entry, string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
                return true;

            for (var i = 0; i < entry.Values.Length; i++)
            {
                ValueSettingEntry settingEntry = _settingsProvider.GetEntrySettings(GameName, entry.Name, i);
                string valueString = GetValueString(entry.Values[i].Value, entry.Values[i].Type, settingEntry.IsHex);

                if (valueString.Contains(searchText))
                    return true;
            }

            return false;
        }

        #endregion

        private IList<TreeNode<ConfigurationEntry>> GetNeighbourNodes(TreeNode<ConfigurationEntry> node)
        {
            return node.Parent?.Nodes ?? _fullTreeView.Nodes;
        }

        private int CountEntries(TreeNode<ConfigurationEntry> node)
        {
            if (node.Nodes.Count <= 0)
                return 1;

            var count = 1;
            foreach (TreeNode<ConfigurationEntry> child in node.Nodes)
                count += CountEntries(child);

            return count + 1;
        }

        private string GetValueString(object value, ValueType type, bool isHex)
        {
            switch (type)
            {
                case ValueType.String:
                    return $"{value}";

                case ValueType.Int:
                    return isHex ? $"0x{value:X8}" : $"{value}";

                case ValueType.Float:
                    return $"{value}";

                default:
                    throw new InvalidOperationException($"Unknown value type {type}.");
            }
        }
    }
}
