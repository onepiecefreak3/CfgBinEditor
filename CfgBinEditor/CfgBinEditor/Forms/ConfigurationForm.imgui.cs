using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CfgBinEditor.resources;
using ImGui.Forms.Controls;
using ImGui.Forms.Controls.Layouts;
using ImGui.Forms.Controls.Tree;
using ImGui.Forms.Models;
using Logic.Business.CfgBinValueSettingsManagement.Contract;
using Logic.Domain.Level5.Contract.DataClasses;
using Veldrid;

namespace CfgBinEditor.Forms
{
    public partial class ConfigurationForm
    {
        private const string NoGame_ = "None";

        private StackLayout _contentLayout;

        private StackLayout _gameLayout;
        private StackLayout _valuesLayout;

        private TreeView<ConfigurationEntry> _flatEntryTree;
        private TreeView<ConfigurationEntry> _nestedTreeView;

        private ComboBox<string> _gameComboBox;
        private Button _gameAddButton;

        private Panel _configContent;

        private void InitializeComponent(Configuration config, IValueSettingsProvider settingsProvider)
        {
            _contentLayout = new StackLayout { Alignment = Alignment.Horizontal, ItemSpacing = 5 };

            _gameLayout = new StackLayout { Alignment = Alignment.Horizontal, ItemSpacing = 5, Size = new Size(SizeValue.Parent, SizeValue.Content) };
            _valuesLayout = new StackLayout { Alignment = Alignment.Vertical, ItemSpacing = 5 };

            _flatEntryTree = new TreeView<ConfigurationEntry> { Size = new Size(SizeValue.Relative(.4f), SizeValue.Parent) };
            _nestedTreeView = new TreeView<ConfigurationEntry> { Size = new Size(SizeValue.Relative(.4f), SizeValue.Parent) };

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

            InitializeFlatEntryNodes(config);
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

        private void InitializeFlatEntryNodes(Configuration config)
        {
            foreach (ConfigurationEntry entry in config.Entries)
                _flatEntryTree.Nodes.Add(new TreeNode<ConfigurationEntry> { Data = entry, Text = entry.Name });
        }

        private int InitializeNestedEntryNodes(Configuration config, int index = 0, string? endNodeName = null, TreeNode<ConfigurationEntry>? parentNode = null)
        {
            for (; index < config.Entries.Length; index++)
            {
                ConfigurationEntry entry = config.Entries[index];
                if (entry.Name == endNodeName)
                    break;

                var entryNode = new TreeNode<ConfigurationEntry> { Data = entry, Text = entry.Name };

                int beginIndex = entry.Name.LastIndexOf("_BEGIN", StringComparison.Ordinal);
                int begIndex = entry.Name.LastIndexOf("_BEG", StringComparison.Ordinal);
                int bgnIndex = entry.Name.LastIndexOf("_BGN", StringComparison.Ordinal);
                if (beginIndex >= 0 || begIndex >= 0 || bgnIndex >= 0)
                {
                    int bIndex = beginIndex < 0 ? begIndex < 0 ? bgnIndex : begIndex : beginIndex;

                    entryNode.Text = entry.Name[..bIndex];
                    index = InitializeNestedEntryNodes(config, index + 1, entryNode.Text + "_END", entryNode);
                }

                if (parentNode == null)
                    _nestedTreeView.Nodes.Add(entryNode);
                else
                    parentNode.Nodes.Add(entryNode);
            }

            return index;
        }
    }
}
