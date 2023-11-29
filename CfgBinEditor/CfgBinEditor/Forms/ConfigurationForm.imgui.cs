using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CfgBinEditor.resources;
using ImGui.Forms.Controls;
using ImGui.Forms.Controls.Layouts;
using ImGui.Forms.Controls.Menu;
using ImGui.Forms.Controls.Tree;
using ImGui.Forms.Models;
using Logic.Business.CfgBinValueSettingsManagement.Contract;
using Logic.Business.CfgBinValueSettingsManagement.Contract.DataClasses;
using Logic.Domain.Level5.Contract.DataClasses;
using Quartz.Xml;
using Rectangle = Veldrid.Rectangle;
using Size = ImGui.Forms.Models.Size;
using ValueType = Logic.Domain.Level5.Contract.DataClasses.ValueType;

namespace CfgBinEditor.Forms
{
    public partial class ConfigurationForm
    {
        private const string NoGame_ = "None";

        private StackLayout _contentLayout;

        private StackLayout _entryLayout;
        private StackLayout _searchLayout;
        private StackLayout _gameLayout;
        private StackLayout _valuesLayout;

        private TextBox _searchTextBox;
        private ImageButton _clearSearchButton;

        private TreeView<ConfigurationEntry> _entryTreeView;
        private TreeView<ConfigurationEntry> _filteredEntryTreeView;
        private ContextMenu _treeViewContextMenu;

        private MenuBarButton _duplicateButton;

        private ComboBox<string> _gameComboBox;
        private Button _gameAddButton;

        private Panel _configContent;

        private void InitializeComponent(Configuration config, IValueSettingsProvider settingsProvider)
        {
            _contentLayout = new StackLayout { Alignment = Alignment.Horizontal, ItemSpacing = 5 };

            _entryLayout = new StackLayout { Alignment = Alignment.Vertical, ItemSpacing = 5, Size = new Size(SizeValue.Relative(.4f), SizeValue.Parent) };
            _searchLayout = new StackLayout { Alignment = Alignment.Horizontal, ItemSpacing = 5, Size = Size.WidthAlign };
            _gameLayout = new StackLayout { Alignment = Alignment.Horizontal, ItemSpacing = 5, Size = new Size(SizeValue.Parent, SizeValue.Content) };
            _valuesLayout = new StackLayout { Alignment = Alignment.Vertical, ItemSpacing = 5 };

            _duplicateButton = new MenuBarButton { Text = LocalizationResources.CfgBinEntryDuplicateCaption };

            _treeViewContextMenu = new ContextMenu
            {
                Items =
                {
                    _duplicateButton
                }
            };

            _searchTextBox = new TextBox { Placeholder = LocalizationResources.CfgBinEntrySearchPlaceholderCaption };
            _clearSearchButton = new ImageButton { Image = ImageResources.Close };

            _entryTreeView = new TreeView<ConfigurationEntry> { ContextMenu = _treeViewContextMenu };
            _filteredEntryTreeView = new TreeView<ConfigurationEntry>();

            _gameComboBox = new ComboBox<string>();
            _gameAddButton = new Button { Text = LocalizationResources.GameAddButtonCaption };

            _configContent = new Panel();

            _searchLayout.Items.Add(new StackItem(_searchTextBox) { VerticalAlignment = VerticalAlignment.Center });
            _searchLayout.Items.Add(_clearSearchButton);

            _entryLayout.Items.Add(_searchLayout);
            _entryLayout.Items.Add(_entryTreeView);

            _gameLayout.Items.Add(_gameComboBox);
            _gameLayout.Items.Add(_gameAddButton);

            _valuesLayout.Items.Add(_gameLayout);
            _valuesLayout.Items.Add(_configContent);

            _contentLayout.Items.Add(_entryLayout);
            _contentLayout.Items.Add(_valuesLayout);

            InitializeGames(settingsProvider);

            InitializeEntryNodes(config);
        }

        public override Size GetSize()
        {
            return Size.Parent;
        }

        protected override void UpdateInternal(Rectangle contentRect)
        {
            _contentLayout.Update(contentRect);
        }

        private void InitializeGames(IValueSettingsProvider settingsProvider)
        {
            _gameComboBox.Items.Add(NoGame_);
            _gameComboBox.SelectedItem = _gameComboBox.Items[0];

            foreach (string game in settingsProvider.GetGames())
                _gameComboBox.Items.Add(game);
        }

        private void InitializeEntryNodes(Configuration config)
        {
            IList<TreeNode<ConfigurationEntry>> nodeTree = CreateNodeTree(config, string.Empty);

            foreach (TreeNode<ConfigurationEntry> node in nodeTree)
                _entryTreeView.Nodes.Add(node);
        }

        private void ResetNodesChangeState(IList<TreeNode<ConfigurationEntry>> nodes)
        {
            foreach (TreeNode<ConfigurationEntry> node in nodes)
            {
                node.TextColor = ColorResources.TextDefault;

                if (node.Nodes.Count > 0)
                    ResetNodesChangeState(node.Nodes);
            }
        }

        private IList<TreeNode<ConfigurationEntry>> CreateNodeTree(Configuration config, string searchText)
        {
            var result = new List<TreeNode<ConfigurationEntry>>();

            for (var index = 0; index < config.Entries.Length; index++)
            {
                TreeNode<ConfigurationEntry> entryNode = CreateNode(config, ref index, ColorResources.TextDefault, searchText);

                if (entryNode.Nodes.Count > 0 || IsEntrySearched(entryNode.Data, searchText))
                    result.Add(entryNode);
            }

            return result;
        }

        private int CreateNodeTree(Configuration config, int index, string endNodeName, TreeNode<ConfigurationEntry> parentNode, Color textColor, string searchText)
        {
            for (; index < config.Entries.Length; index++)
            {
                if (config.Entries[index].Name == endNodeName)
                    break;

                TreeNode<ConfigurationEntry> entryNode = CreateNode(config, ref index, textColor, searchText);

                if (entryNode.Nodes.Count > 0 || IsEntrySearched(entryNode.Data, searchText))
                    parentNode.Nodes.Add(entryNode);
            }

            return index;
        }

        private TreeNode<ConfigurationEntry> CreateNode(Configuration config, ref int index, Color textColor, string searchText)
        {
            ConfigurationEntry entry = config.Entries[index];

            var entryNode = new TreeNode<ConfigurationEntry> { Data = entry, Text = entry.Name, TextColor = textColor };

            int beginIndex = entry.Name.LastIndexOf("_BEGIN", StringComparison.Ordinal);
            int begIndex = entry.Name.LastIndexOf("_BEG", StringComparison.Ordinal);
            int bgnIndex = entry.Name.LastIndexOf("_BGN", StringComparison.Ordinal);
            if (beginIndex < 0 && begIndex < 0 && bgnIndex < 0)
                return entryNode;

            int bIndex = beginIndex < 0 ? begIndex < 0 ? bgnIndex : begIndex : beginIndex;

            entryNode.Text = entry.Name[..bIndex];
            index = CreateNodeTree(config, index + 1, entryNode.Text + "_END", entryNode, textColor, searchText);

            return entryNode;
        }

        private TreeNode<ConfigurationEntry> GetLastNode(TreeNode<ConfigurationEntry> node)
        {
            return GetNeighbourNodes(node)[^1];
        }

        private IList<TreeNode<ConfigurationEntry>> GetNeighbourNodes(TreeNode<ConfigurationEntry> node)
        {
            return node.Parent?.Nodes ?? _entryTreeView.Nodes;
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

        private bool IsEntrySearched(ConfigurationEntry entry, string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
                return true;

            for (var i = 0; i < entry.Values.Length; i++)
            {
                ValueSettingEntry settingEntry = _settingsProvider.GetEntrySettings(GetCurrentGame(), entry.Name, i);
                string valueString = GetValueString(entry.Values[i].Value, entry.Values[i].Type, settingEntry.IsHex);

                if (valueString.Contains(searchText))
                    return true;
            }

            return false;
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
