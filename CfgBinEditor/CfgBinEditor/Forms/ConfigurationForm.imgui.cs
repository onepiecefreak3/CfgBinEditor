using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using CfgBinEditor.resources;
using ImGui.Forms.Controls;
using ImGui.Forms.Controls.Layouts;
using ImGui.Forms.Controls.Menu;
using ImGui.Forms.Controls.Tree;
using ImGui.Forms.Models;
using Logic.Business.CfgBinValueSettingsManagement.Contract;
using Logic.Domain.Level5.Contract.DataClasses;
using Rectangle = Veldrid.Rectangle;
using Size = ImGui.Forms.Models.Size;

namespace CfgBinEditor.Forms
{
    public partial class ConfigurationForm
    {
        private const string NoGame_ = "None";

        private StackLayout _contentLayout;

        private StackLayout _gameLayout;
        private StackLayout _valuesLayout;

        private TreeView<ConfigurationEntry> _nestedTreeView;
        private ContextMenu _treeViewContextMenu;

        private MenuBarButton _duplicateButton;

        private ComboBox<string> _gameComboBox;
        private Button _gameAddButton;

        private Panel _configContent;

        private void InitializeComponent(Configuration config, IValueSettingsProvider settingsProvider)
        {
            _contentLayout = new StackLayout { Alignment = Alignment.Horizontal, ItemSpacing = 5 };

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

            _nestedTreeView = new TreeView<ConfigurationEntry>
            {
                Size = new Size(SizeValue.Relative(.4f), SizeValue.Parent),
                ContextMenu = _treeViewContextMenu
            };

            _gameComboBox = new ComboBox<string>();
            _gameAddButton = new Button { Text = LocalizationResources.GameAddButtonCaption };

            _configContent = new Panel();

            _gameLayout.Items.Add(_gameComboBox);
            _gameLayout.Items.Add(_gameAddButton);

            _valuesLayout.Items.Add(_gameLayout);
            _valuesLayout.Items.Add(_configContent);

            _contentLayout.Items.Add(_nestedTreeView);
            _contentLayout.Items.Add(_valuesLayout);

            InitializeGames(settingsProvider);

            InitializeNestedEntryNodes(config);
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

        private void InitializeNestedEntryNodes(Configuration config)
        {
            IList<TreeNode<ConfigurationEntry>> nodeTree = CreateNodeTree(config);

            foreach (TreeNode<ConfigurationEntry> node in nodeTree)
                _nestedTreeView.Nodes.Add(node);
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

        private IList<TreeNode<ConfigurationEntry>> CreateNodeTree(Configuration config)
        {
            var result = new List<TreeNode<ConfigurationEntry>>();

            for (var index = 0; index < config.Entries.Length; index++)
            {
                TreeNode<ConfigurationEntry> entryNode = CreateNode(config, ref index, ColorResources.TextDefault);

                result.Add(entryNode);
            }

            return result;
        }

        private int CreateNodeTree(Configuration config, int index, string endNodeName, TreeNode<ConfigurationEntry> parentNode, Color textColor)
        {
            for (; index < config.Entries.Length; index++)
            {
                if (config.Entries[index].Name == endNodeName)
                    break;

                TreeNode<ConfigurationEntry> entryNode = CreateNode(config, ref index, textColor);

                parentNode.Nodes.Add(entryNode);
            }

            return index;
        }

        private TreeNode<ConfigurationEntry> CreateNode(Configuration config, ref int index, Color textColor)
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
            index = CreateNodeTree(config, index + 1, entryNode.Text + "_END", entryNode, textColor);

            return entryNode;
        }

        private TreeNode<ConfigurationEntry> GetNextNode(TreeNode<ConfigurationEntry> node)
        {
            IList<TreeNode<ConfigurationEntry>> nodes = node.Parent?.Nodes ?? _nestedTreeView.Nodes;

            int nodeIndex = nodes.IndexOf(node) + 1;
            if (nodeIndex < nodes.Count)
                return nodes[nodeIndex];

            return null;
        }

        private TreeNode<ConfigurationEntry> GetLastNode(TreeNode<ConfigurationEntry> node)
        {
            return GetNeighbourNodes(node)[^1];
        }

        private IList<TreeNode<ConfigurationEntry>> GetNeighbourNodes(TreeNode<ConfigurationEntry> node)
        {
            return node.Parent?.Nodes ?? _nestedTreeView.Nodes;
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
    }
}
