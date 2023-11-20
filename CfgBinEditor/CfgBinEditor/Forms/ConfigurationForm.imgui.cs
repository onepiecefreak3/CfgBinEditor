using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGui.Forms.Controls;
using ImGui.Forms.Controls.Layouts;
using ImGui.Forms.Controls.Tree;
using ImGui.Forms.Models;
using Kontract.Kanvas.Configuration;
using Logic.Domain.Level5.Contract.DataClasses;
using Veldrid;

namespace CfgBinEditor.Forms
{
    public partial class ConfigurationForm
    {
        private StackLayout _contentLayout;

        private TreeView<ConfigurationEntry> _flatEntryTree;
        private TreeView<ConfigurationEntry> _nestedTreeView;
        private Panel _configContent;

        private void InitializeComponent(Configuration config)
        {
            _contentLayout = new StackLayout { Alignment = Alignment.Horizontal, ItemSpacing = 5 };

            _flatEntryTree = new TreeView<ConfigurationEntry> { Size = new Size(SizeValue.Relative(.4f), SizeValue.Parent) };
            _nestedTreeView = new TreeView<ConfigurationEntry> { Size = new Size(SizeValue.Relative(.4f), SizeValue.Parent) };
            _configContent = new Panel();

            _contentLayout.Items.Add(_nestedTreeView);
            _contentLayout.Items.Add(_configContent);

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
                if (beginIndex >= 0 || begIndex >= 0)
                {
                    entryNode.Text = entry.Name[..begIndex];
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
